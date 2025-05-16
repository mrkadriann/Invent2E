// In InventoryManagement.Models/SupplierDetailViewModel.cs

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc; // Required for IUrlHelper

namespace InventoryManagement.Models
{
    public class SupplierDetailViewModel
    {
        public Supplier Supplier { get; private set; }
        public List<ProductForView> SupplierProducts { get; private set; }
        public SupplierContact PrimaryContact { get; private set; }
        public List<SupplierContact> OtherContacts { get; private set; }

        public string SupplierAvatarColor { get; private set; }
        public string SupplierAvatarInitial { get; private set; }
        public string PrimaryContactAvatarColor { get; private set; }
        public string PrimaryContactAvatarInitial { get; private set; }
        public string CurrencySymbol { get; private set; }
        public decimal TotalPayables { get; set; }

        public SupplierDetailViewModel(Supplier supplier, IUrlHelper urlHelper = null)
        {
            Supplier = supplier;
            SupplierProducts = new List<ProductForView>();
            OtherContacts = new List<SupplierContact>();

            ProcessSupplierData(urlHelper);
        }

        private void ProcessSupplierData(IUrlHelper urlHelper)
        {
            // 1. Contact Logic (remains the same)
            if (!string.IsNullOrWhiteSpace(Supplier.PersonName) ||
                !string.IsNullOrWhiteSpace(Supplier.Email) ||
                !string.IsNullOrWhiteSpace(Supplier.PhoneNumber))
            {
                PrimaryContact = new SupplierContact
                {
                    ContactPersonName = Supplier.PersonName,
                    Email = Supplier.Email,
                    PhoneNumber = Supplier.PhoneNumber,
                    SupplierCompanyId = Supplier.SupplierId
                };
            }
            else
            {
                PrimaryContact = null;
            }

            if (Supplier.SupplierContacts != null && Supplier.SupplierContacts.Any())
            {
                OtherContacts = Supplier.SupplierContacts.ToList();
            }
            else
            {
                OtherContacts = new List<SupplierContact>();
            }

            // 2. Avatars (remains the same)
            SupplierAvatarInitial = GetInitial(Supplier.CompanyName, "S");
            SupplierAvatarColor = GetColorForInitial(SupplierAvatarInitial);

            if (PrimaryContact != null && !string.IsNullOrWhiteSpace(PrimaryContact.ContactPersonName))
            {
                PrimaryContactAvatarInitial = GetInitial(PrimaryContact.ContactPersonName, "C");
                PrimaryContactAvatarColor = GetColorForInitial(PrimaryContactAvatarInitial);
            }
            else
            {
                PrimaryContactAvatarInitial = "?";
                PrimaryContactAvatarColor = GetColorForInitial("?");
            }

            // 3. Product Image URL Logic (Corrected)
            if (Supplier.Products != null)
            {
                foreach (var p in Supplier.Products) // 'p' is a Product entity
                {
                    string imageUrl = "/images/placeholder-product.png"; // Default to placeholder

                    // Strategy:
                    // 1. Try the explicitly set Product.PrimaryImage first.
                    // 2. If not set, try finding the image with the lowest ImageOrder from Product.AllImages.
                    // 3. If still nothing, default to placeholder.

                    if (p.PrimaryImage != null) // Check the direct navigation property
                    {
                        // Ensure SupplierController includes .ThenInclude(pr => pr.PrimaryImage)
                        imageUrl = $"/Image/GetImage/{p.PrimaryImage.ImageId}";
                    }
                    else if (p.AllImages != null && p.AllImages.Any()) // Fallback to AllImages using ImageOrder
                    {
                        // Ensure SupplierController includes .ThenInclude(pr => pr.AllImages)
                        ImageData bestOrderedImage = p.AllImages
                                                     .Where(img => img.ImageOrder.HasValue)
                                                     .OrderBy(img => img.ImageOrder)
                                                     .FirstOrDefault();
                        if (bestOrderedImage != null)
                        {
                            imageUrl = $"/Image/GetImage/{bestOrderedImage.ImageId}";
                        }
                        // Optional: If no ordered images, you could take p.AllImages.FirstOrDefault()
                        // else
                        // {
                        //     ImageData anyImage = p.AllImages.FirstOrDefault();
                        //     if (anyImage != null)
                        //     {
                        //         imageUrl = $"/Image/GetImage/{anyImage.ImageId}";
                        //     }
                        // }
                    }

                    SupplierProducts.Add(new ProductForView
                    {
                        Id = p.ItemId,
                        Name = p.ItemName,
                        Category = p.Category?.CategoryName,
                        Price = p.Description?.RetailPrice,
                        Stock = p.Quantity?.Qty,
                        StockStatus = CalculateStockStatus(p.Quantity?.Qty),
                        ImageUrl = imageUrl
                    });
                }
            }
            CurrencySymbol = GetCurrencySymbol(Supplier.Currency);
        }

        // Helper methods (GetInitial, GetColorForInitial, GetCurrencySymbol, CalculateStockStatus) remain the same
        private static string GetInitial(string name, string fallback = "?")
        {
            return !string.IsNullOrWhiteSpace(name) ? name.Trim().Substring(0, 1).ToUpper() : fallback;
        }

        private static string GetColorForInitial(string initial)
        {
            if (string.IsNullOrWhiteSpace(initial) || initial == "?") return "#cccccc";
            int charCode = (int)initial[0];
            var colors = new[] {
                "#4A90E2", "#50E3C2", "#F5A623", "#BD10E0", "#9013FE", "#D0021B",
                "#F8E71C", "#7ED321", "#B8E986", "#417505", "#BF5AF2", "#FF9500",
                "#FFCC00", "#FF3B30", "#007AFF", "#34C759", "#5856D6", "#FF2D55"
            };
            return colors[charCode % colors.Length];
        }

        private static string GetCurrencySymbol(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode)) return "$";
            string normalizedCode = currencyCode.ToUpper().Replace("PESO", "").Replace("(", "").Replace(")", "").Trim();
            switch (normalizedCode)
            {
                case "PHP": case "P": return "₱";
                case "USD": return "$";
                case "EUR": return "€";
                case "GBP": return "£";
                case "JPY": return "¥";
                default: return currencyCode;
            }
        }

        private static string CalculateStockStatus(int? quantity)
        {
            if (!quantity.HasValue || quantity.Value <= 0) return "Out of Stock";
            if (quantity.Value <= 20) return "Danger";
            if (quantity.Value <= 30) return "Low";
            if (quantity.Value <= 50) return "Normal";
            return "Overstocked";
        }
    }
}