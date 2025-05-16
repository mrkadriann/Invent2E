using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    public class SupplierController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Supplier";
            return View();
        }
    }
}
