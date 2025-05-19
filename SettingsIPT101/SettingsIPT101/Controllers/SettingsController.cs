using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SettingsIPT101.Models;
using SettingsIPT101.SettData; // Make sure this matches your namespace

namespace SettingsIPT101.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            var vm = new SettingsViewModel();

            if (settings != null)
            {
                vm.CompanyName = settings.CompanyName;
                vm.BusinessType = settings.BusinessType;
                vm.Email = settings.Email;
                vm.Phone = settings.Phone;
                vm.Address = settings.Address;
                vm.Language = settings.Language;
                vm.Currency = settings.Currency;
                vm.EnableTax = settings.EnableTax;
                vm.TaxRate = settings.TaxRate;
                vm.NotifyLowStock = settings.NotifyLowStock;
                vm.NotifyOutOfStock = settings.NotifyOutOfStock;
                vm.NotifyReplenish = settings.NotifyReplenish;
                vm.Employee = settings.Employee;
                vm.Customer = settings.Customer;
                vm.DisableFeature1 = settings.DisableFeature1;
                vm.DisableFeature2 = settings.DisableFeature2;
                // Set the photo URL for display
                vm.CurrentPhotoUrl = settings.CompanyPhotoPath;
            }

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> Index(SettingsViewModel vm)
        {

            System.Diagnostics.Debug.WriteLine("DB Connection: " + _context.Database.GetDbConnection().ConnectionString);

            if (!ModelState.IsValid)
                return View(vm);
            if (!ModelState.IsValid)
            {
                // This will show you all validation errors in the Output window
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
                }
                return View(vm);
            }

            var settings = await _context.Settings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new Settings();
                _context.Settings.Add(settings);
            }

            // Map all direct properties
            settings.CompanyName = vm.CompanyName;
            settings.BusinessType = vm.BusinessType;
            settings.Email = vm.Email;
            settings.Phone = vm.Phone;
            settings.Address = vm.Address;
            settings.Language = vm.Language;
            settings.Currency = vm.Currency;
            settings.EnableTax = vm.EnableTax;
            settings.TaxRate = vm.TaxRate;
            settings.NotifyLowStock = vm.NotifyLowStock;
            settings.NotifyOutOfStock = vm.NotifyOutOfStock;
            settings.NotifyReplenish = vm.NotifyReplenish;
            settings.Employee = vm.Employee;
            settings.Customer = vm.Customer;
            settings.DisableFeature1 = vm.DisableFeature1;
            settings.DisableFeature2 = vm.DisableFeature2;

            // Handle photo upload
            if (vm.CompanyPhoto != null && vm.CompanyPhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid() + Path.GetExtension(vm.CompanyPhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.CompanyPhoto.CopyToAsync(stream);
                }

                settings.CompanyPhotoPath = "/images/" + fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
