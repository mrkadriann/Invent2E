using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace InventoryManagement.Controllers
{
    public class SupplierController : Controller
    {
        private readonly InventoryDbContext _context;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        private readonly List<string> _predefinedCities = new List<string>
        {
            "Quezon City",
            "Manila",
            "Pasig",
            "Marikina",
            "Taguig"
        };
        public SupplierController(InventoryDbContext context, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor)
        {
            _context = context;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString, string locationFilter, string statusFilter, string currentFilter, int? pageNumber)
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
            ViewData["CurrentTopSearch"] = searchString;
            ViewData["CurrentLocationFilter"] = locationFilter;
            ViewData["CurrentStatusFilter"] = statusFilter;

            var supplierQuery = _context.Suppliers
                .Include(s => s.Products)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string lowerSearchString = searchString.ToLower().Trim();
                supplierQuery = supplierQuery.Where(s =>
                    (s.CompanyName != null && s.CompanyName.ToLower().Contains(lowerSearchString)) ||
                    (s.Email != null && s.Email.ToLower().Contains(lowerSearchString)) ||
                    (s.PhoneNumber != null && s.PhoneNumber.Contains(lowerSearchString)) ||
                    (s.PersonName != null && s.PersonName.ToLower().Contains(lowerSearchString)) ||
                    (s.Address != null && s.Address.ToLower().Contains(lowerSearchString))
                );
            }

            if (!string.IsNullOrEmpty(locationFilter) && _predefinedCities.Contains(locationFilter))
            {
                string lowerLocationFilter = locationFilter.ToLower();
                supplierQuery = supplierQuery.Where(s => s.Address != null && s.Address.ToLower().Contains(lowerLocationFilter));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                supplierQuery = supplierQuery.Where(s => s.PortalStatus != null && s.PortalStatus == statusFilter);
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

            ViewBag.Locations = _predefinedCities.OrderBy(c => c).ToList();

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

        // Updating Supplier

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .Include(s => s.SupplierContacts)
                .FirstOrDefaultAsync(s => s.SupplierId == id);


            if (supplier == null)
            {
                return NotFound();
            }

            var primaryContact = supplier.SupplierContacts?.FirstOrDefault();
            var viewModel = new AddSupplierViewModel()
            {
                SupplierId = supplier.SupplierId,
                CompanyName = supplier.CompanyName,
                PersonName = supplier.PersonName,
                Department = supplier.Department,
                Email = supplier.Email,
                PhoneNumber = supplier.PhoneNumber,
                Address = supplier.Address,
                Currency = supplier.Currency,
                PaymentMethod = supplier.PaymentMethod,
                Courier = supplier.Courier,
                PortalStatus = supplier.PortalStatus,
                HasExistingProfileImage = supplier.ProfileImage != null && supplier.ProfileImage.Length > 0,

                OtherContacts = new List<SupplierContactViewModel>()


            };

            if (supplier.SupplierContacts != null)
            {
                foreach (var contactEntity in supplier.SupplierContacts)
                {
                    viewModel.OtherContacts.Add(new SupplierContactViewModel
                    {
                        ContactId = contactEntity.ContactId,
                        Name = contactEntity.ContactPersonName,
                        Email = contactEntity.Email,
                        Phone = contactEntity.PhoneNumber
                    });
                }
            }

            ViewData["Title"] = "Edit Supplier";
            return View(viewModel);
        }


        // POST: Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddSupplierViewModel model)
        {
            if (id != model.SupplierId)
            {
                return BadRequest("Mismatched supplier ID.");
            }

            // If no new image is uploaded, ProfileImageFile will be null.
            // We don't want validation to fail for this if an image already exists or none is required.
            if (model.ProfileImageFile == null)
            {
                ModelState.Remove(nameof(model.ProfileImageFile));
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is INVALID. Errors:");
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"Field: {entry.Key}");
                        foreach (var error in entry.Value.Errors)
                        {
                            Console.WriteLine($"- {error.ErrorMessage}");
                        }
                    }
                }

                ViewData["Title"] = "Edit Supplier";
                // Repopulate HasExistingProfileImage if returning to the view due to validation errors
                if (model.SupplierId > 0)
                {
                    var originalSupplierForView = await _context.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.SupplierId == model.SupplierId);
                    if (originalSupplierForView != null)
                    {
                        model.HasExistingProfileImage = originalSupplierForView.ProfileImage != null && originalSupplierForView.ProfileImage.Length > 0;
                    }
                }
                return View(model);
            }

            // If ModelState.IsValid is true, proceed with update logic
            var supplierToUpdate = await _context.Suppliers
                                             .Include(s => s.SupplierContacts) // Eagerly load existing contacts
                                             .FirstOrDefaultAsync(s => s.SupplierId == id);
            if (supplierToUpdate == null)
            {
                return NotFound();
            }

            // Check for company name uniqueness (excluding the current supplier)
            if (await _context.Suppliers.AnyAsync(s => s.CompanyName == model.CompanyName && s.SupplierId != id))
            {
                ModelState.AddModelError("CompanyName", "Another supplier with this company name already exists.");
                ViewData["Title"] = "Edit Supplier";
                model.HasExistingProfileImage = supplierToUpdate.ProfileImage != null && supplierToUpdate.ProfileImage.Length > 0;
                return View(model);
            }

            // Update scalar properties
            supplierToUpdate.CompanyName = model.CompanyName;
            supplierToUpdate.PersonName = model.PersonName;
            supplierToUpdate.Department = model.Department;
            supplierToUpdate.Email = model.Email;
            supplierToUpdate.PhoneNumber = model.PhoneNumber;
            supplierToUpdate.Address = model.Address;
            supplierToUpdate.Currency = model.Currency;
            supplierToUpdate.PaymentMethod = model.PaymentMethod;
            supplierToUpdate.Courier = model.Courier;
            supplierToUpdate.PortalStatus = model.PortalStatus ?? "Active"; // Ensure PortalStatus has a value

            // Update profile image if a new one was uploaded
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.ProfileImageFile.CopyToAsync(memoryStream);
                    supplierToUpdate.ProfileImage = memoryStream.ToArray();
                }
            }

            // --- Handle "Other Contacts" Updates ---
            // Ensure supplierToUpdate.SupplierContacts is not null (it should be initialized by the constructor)
            supplierToUpdate.SupplierContacts ??= new HashSet<SupplierContact>();
            model.OtherContacts ??= new List<SupplierContactViewModel>(); // Ensure model.OtherContacts is not null

            // Get IDs of submitted contacts that already exist (ContactId > 0)
            var submittedExistingContactIds = model.OtherContacts
                .Where(vmc => vmc.ContactId > 0)
                .Select(vmc => vmc.ContactId)
                .ToList();

            // Identify contacts in the database that are NOT in the submitted list of existing contacts
            var contactsToDelete = supplierToUpdate.SupplierContacts
                .Where(dbContact => !submittedExistingContactIds.Contains(dbContact.ContactId))
                .ToList(); // Materialize to avoid issues during removal

            _context.SupplierContacts.RemoveRange(contactsToDelete); // Remove deleted contacts

            foreach (var vmContact in model.OtherContacts)
            {
                // Skip if it's a placeholder for a new contact and all fields are empty
                if (vmContact.ContactId == 0 &&
                    string.IsNullOrWhiteSpace(vmContact.Name) &&
                    string.IsNullOrWhiteSpace(vmContact.Email) &&
                    string.IsNullOrWhiteSpace(vmContact.Phone))
                {
                    continue;
                }

                if (vmContact.ContactId > 0) // This is an existing contact to update
                {
                    var dbContact = supplierToUpdate.SupplierContacts
                                        .FirstOrDefault(c => c.ContactId == vmContact.ContactId);
                    if (dbContact != null)
                    {
                        // Update properties of the existing contact
                        dbContact.ContactPersonName = vmContact.Name;
                        dbContact.Email = vmContact.Email;
                        dbContact.PhoneNumber = vmContact.Phone;
                        // EF Core will track these changes
                    }
                    // else: Log or handle case where an existing contact ID was submitted but not found (shouldn't happen with correct form data)
                }
                else // This is a new contact (ContactId == 0 or not present)
                {
                    // Only add if at least one identifying piece of information is present
                    if (!string.IsNullOrWhiteSpace(vmContact.Name) ||
                        !string.IsNullOrWhiteSpace(vmContact.Email) ||
                        !string.IsNullOrWhiteSpace(vmContact.Phone))
                    {
                        var newDbContact = new SupplierContact
                        {
                            ContactPersonName = vmContact.Name,
                            Email = vmContact.Email,
                            PhoneNumber = vmContact.Phone,
                            // SupplierCompanyId = supplierToUpdate.SupplierId, // EF Core sets this via navigation property
                            Supplier = supplierToUpdate // This sets the relationship
                        };
                        // Add to the supplier's collection. EF Core will detect this as a new entity.
                        supplierToUpdate.SupplierContacts.Add(newDbContact);
                    }
                }
            }
            // --- End "Other Contacts" Updates ---

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Supplier updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(supplierToUpdate.SupplierId))
                {
                    return NotFound();
                }
                else
                {
                    // Log the concurrency exception
                    ModelState.AddModelError("", "The record you attempted to edit "
                        + "was modified by another user after you got the original value. "
                        + "Your edit operation was canceled and the current values in the database "
                        + "have been displayed. If you still want to edit this record, click "
                        + "the Save button again. Otherwise click the Back to List hyperlink.");

                    model.HasExistingProfileImage = supplierToUpdate.ProfileImage != null && supplierToUpdate.ProfileImage.Length > 0;
                }
            }
            catch (DbUpdateException ex) // Catch more general database update errors
            {
                // Log ex for detailed diagnostics server-side
                Console.WriteLine($"DbUpdateException: {ex.ToString()}");
                ModelState.AddModelError("", $"Unable to save changes. Try again, and if the problem persists, see your system administrator. Error: {ex.GetBaseException().Message}");
                model.HasExistingProfileImage = supplierToUpdate.ProfileImage != null && supplierToUpdate.ProfileImage.Length > 0; // Repopulate for view
            }

            ViewData["Title"] = "Edit Supplier"; // Reset title if returning to view
            return View(model);
        }

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.SupplierId == id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SupplierId == id);

            if (supplier == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Delete Supplier";
            return View(supplier);
        }

        [HttpPost] 
        //[ValidateAntiForgeryToken] // Important for security
        public async Task<IActionResult> PerformDelete(int id) // Renamed for clarity
        {
            var supplier = await _context.Suppliers
                                     .Include(s => s.Products) // Include for Restrict check
                                     .FirstOrDefaultAsync(s => s.SupplierId == id);

            if (supplier == null)
            {

                return Json(new { success = false, message = "Supplier not found or already deleted." });

            }

            var productEntityType = _context.Model.FindEntityType(typeof(Product));
            var supplierForeignKeyOnProduct = productEntityType?.GetForeignKeys()
                .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Supplier) && fk.Properties.Any(p => p.Name == "SupplierId"));

            bool isRestrict = supplierForeignKeyOnProduct?.DeleteBehavior == DeleteBehavior.Restrict;

            if (isRestrict && supplier.Products.Any())
            {
                string errorMessage = $"Cannot delete supplier '{supplier.CompanyName}' because they have {supplier.Products.Count} associated product(s) and the delete rule is set to Restrict.";
                return Json(new { success = false, message = errorMessage });
         
            }

            try
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Supplier '{supplier.CompanyName}' deleted successfully." });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error deleting supplier: {ex.ToString()}");
                string detailedErrorMessage = $"Error deleting supplier '{supplier.CompanyName}'. Please try again.";
                if (ex.InnerException != null && ex.InnerException.Message.Contains("FOREIGN KEY constraint"))
                {
                    detailedErrorMessage = $"Error deleting supplier '{supplier.CompanyName}'. It is still referenced by other records.";
                }
                else if (ex.InnerException != null)
                {
                    detailedErrorMessage = $"Error deleting supplier '{supplier.CompanyName}': {ex.InnerException.Message}";
                }
                return Json(new { success = false, message = detailedErrorMessage });
            }
        }
    }
}
