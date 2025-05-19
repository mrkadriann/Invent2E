using System;

namespace TransactionF.Models
{
    /// <summary>
    /// Represents filter options for orders.
    /// Integration Note: This model is used for filtering orders and can be extended
    /// based on requirements from other modules.
    /// </summary>
    public class FilterOptions
    {
        public string OrderSource { get; set; } // e.g., "Website", "Mobile App", "POS"
        public DateTime? OrderDateFrom { get; set; }
        public DateTime? OrderDateTo { get; set; }
        public string Location { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxOrderValue { get; set; }
        public string Status { get; set; } // e.g., "Quote", "Sample", "Shipment", "Fulfilled"
    }
}