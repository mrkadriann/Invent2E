using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventoryManagement.Models;

namespace InventoryManagement.Models
{
    public class OrderCreateViewModel
    {
        public Order Order { get; set; }
        // Add properties for dropdowns if needed (e.g., Customers, Products)
    }
}