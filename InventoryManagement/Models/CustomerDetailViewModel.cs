using InventoryManagement.Models; // Your main models
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Routing; // For IUrlHelper
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc; // For IActionContextAccessor

namespace InventoryManagement.Models // Or a ViewModels namespace
{
    public class CustomerDetailViewModel
    {
        public Customer Customer { get; set; }
        public string CustomerAvatarInitial { get; set; }
        public string CustomerAvatarColor { get; set; }

        // Placeholder for future data
        public List<DummyTransaction> RecentTransactions { get; set; } = new List<DummyTransaction>();
        public DummyRunningSaleOrder CurrentRunningSaleOrder { get; set; }

        // Constructor to initialize from Customer entity and helpers
        public CustomerDetailViewModel(Customer customer, IUrlHelper urlHelper)
        {
            Customer = customer;

            if (!string.IsNullOrEmpty(customer.CustomerName))
            {
                CustomerAvatarInitial = customer.CustomerName.Substring(0, 1).ToUpper();
                CustomerAvatarColor = GetAvatarColor(CustomerAvatarInitial);
            }
            else
            {
                CustomerAvatarInitial = "C";
                CustomerAvatarColor = GetAvatarColor("C");
            }

            // Initialize dummy data
            CurrentRunningSaleOrder = new DummyRunningSaleOrder
            {
                OrderId = "SO654",
                ProductName = "SAMBA OG SHOES",
                Amount = 500.00m,
                Status = "Unpaid",
                ShippingTo = "21 Rand st., North Fairview Quezon City",
                Courier = "JNT",
                Quote = true,
                Started = false
            };
        }

        private string GetAvatarColor(string initial)
        {
            if (string.IsNullOrEmpty(initial)) return "#cccccc";
            int charCode = (int)initial[0];
            var colors = new[] { "#3b82f6", "#8b5cf6", "#10b981", "#f59e0b", "#ef4444", "#6366f1", "#ec4899", "#f43f5e" };
            return colors[charCode % colors.Length];
        }
    }

    public class DummyTransaction
    {
        public string Id { get; set; }
        public System.DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class DummyRunningSaleOrder
    {
        public string OrderId { get; set; }
        public string ProductName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } // e.g., "Unpaid", "Paid"
        public string ShippingTo { get; set; }
        public string Courier { get; set; }
        public bool Quote { get; set; } // To determine button state/text
        public bool Started { get; set; } // To determine button state/text
    }
}