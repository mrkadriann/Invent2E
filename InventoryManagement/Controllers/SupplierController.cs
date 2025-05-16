using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InventoryManagement.Controllers
{
    public class SupplierController : Controller
    {
        private readonly InventoryDbContext _context;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        public SupplierController(InventoryDbContext context, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor)
        {
            _context = context;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["EmailSortParam"] = sortOrder == "Email" ? "email_desc" : "Email";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var supplierQuery = _context.Suppliers
                .Include(s => s.Products)
                .Include(s => s.SupplierContacts)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string lowerSearchString = searchString.ToLower();
                supplierQuery = supplierQuery.Where(s =>
                s.CompanyName.ToLower().Contains(lowerSearchString) ||
                s.Email.ToLower().Contains(lowerSearchString) ||
                s.PhoneNumber.Contains(searchString) ||
                s.PersonName.ToLower().Contains(lowerSearchString)
                );
            }

            switch (sortOrder)
            {
                case "name_desc":
                    supplierQuery = supplierQuery.OrderByDescending(s => s.CompanyName);
                    break;
                case "Email":
                    supplierQuery = supplierQuery.OrderBy(s => s.Email);
                    break;
                case "email_desc":
                    supplierQuery = supplierQuery.OrderByDescending(s => s.Email);
                    break;
                default:
                    supplierQuery = supplierQuery.OrderBy(s => s.CompanyName);
                    break;
            }

            var finalSuppliers = await supplierQuery.AsNoTracking().ToListAsync();

            return View(finalSuppliers);
        }

        public async Task<IActionResult> GetSupplierImage(int id)
        {
            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SupplierId == id);

            if (supplier != null && supplier.ProfileImage != null && supplier.ProfileImage.Length > 0)
            {
                string contentType = "image/jpeg";
                if (IsPng(supplier.ProfileImage)) contentType = "image/png"; 
                return File(supplier.ProfileImage, contentType);

            }

            return NotFound();


        }

        private bool IsPng(byte[] bytes)
        {
            return bytes.Length > 8 &&
                   bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                   bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
        }


        public IActionResult Create()
        {
            var viewModel = new AddSupplierViewModel();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddSupplierViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Suppliers.AnyAsync(s => s.CompanyName == model.CompanyName))
                {
                    ModelState.AddModelError("CompanyName", "A supplier with this company name already exists.");
                    return View(model);
                }

                var supplier = new Supplier
                {
                    CompanyName = model.CompanyName,
                    PersonName = model.PersonName,
                    Department = model.Department,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Currency = model.Currency,
                    PaymentMethod = model.PaymentMethod,
                    Courier = model.Courier,
                    PortalStatus = model.PortalStatus ?? "Active",
                    Products = new HashSet<Product>(),
                    SupplierContacts = new HashSet<SupplierContact>(),
                };

                if (model.ProfileImageFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ProfileImageFile.CopyToAsync(memoryStream);
                        supplier.ProfileImage = memoryStream.ToArray();
                    }
                }
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(model.PrimaryContactName) &&
                    !string.IsNullOrWhiteSpace(model.PrimaryContactEmail) &&
                    !string.IsNullOrWhiteSpace(model.PrimaryContactPhone))
                {
                    var supplierContact = new SupplierContact
                    {
                        ContactPersonName = model.PrimaryContactName,
                        Email = model.PrimaryContactEmail,
                        PhoneNumber = model.PrimaryContactPhone,
                        SupplierCompanyId = supplier.SupplierId,
                        Supplier = supplier
                    };
                    _context.SupplierContacts.Add(supplierContact);
                    await _context.SaveChangesAsync();
                }
                TempData["SuccessMessage"] = "Supplier added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> GetSupplierDetailsPartial(int id)
        {
            var supplierEntity = await _context.Suppliers
                .Include(s => s.SupplierContacts)
                .Include(s => s.Products)
                    .ThenInclude(p => p.PrimaryImage) 
                .Include(s => s.Products)
                    .ThenInclude(p => p.AllImages)
                .Include(s => s.Products)
                    .ThenInclude(p => p.Category)
                .Include(s => s.Products)
                    .ThenInclude(p => p.Quantity)
                .Include(s => s.Products)
                    .ThenInclude(p => p.Description)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SupplierId == id);

            if (supplierEntity == null)
            {
                return NotFound();
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var viewModel = new SupplierDetailViewModel(supplierEntity, urlHelper);

            return PartialView("_SupplierDetail", viewModel);
        }


    }
}
