using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementAPI.Models.CoreModels.SupplierModels
{
    [PrimaryKey(nameof(SupplierID), nameof(ProductID))]
    public class SupplierProduct
    {
        [Range(1, int.MaxValue)]
        public int SupplierID { get; set; }
        public Supplier Supplier { get; set; } = null!;
        
        [Range(1, int.MaxValue)]
        public int ProductID { get; set; }
        public Product Product { get; set; } = null!;

        
        [StringLength(100)]
        public string? SupplierSku { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal UnitCost { get; set; }

        [Range(0, 3650)]
        public int LeadTimeDays { get; set; }

        [Range(1, int.MaxValue)]
        public int MinimumOrderQuantity { get; set; } = 1;
        
        public bool IsPreferred { get; set; }
       
        public bool IsActive { get; set; } = true;

        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = [];
    }
}
