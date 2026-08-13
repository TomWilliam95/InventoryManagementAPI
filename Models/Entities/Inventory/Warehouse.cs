using InventoryManagementAPI.Models.CoreModels.MovementModels;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class Warehouse
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }
        
       
        [Required]
        [StringLength(200)]
        public required string Address { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string City { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string State { get; set; }

        [Required]
        [StringLength(20)]
        public required string ZipCode { get; set; }

        [Required]
        [StringLength(100)]
        public required string Country { get; set; }

       
        [Required]
        public bool IsActive { get; set; }

        
        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime Updated { get; set; }

        
        [Required]
        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

       
        public virtual ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();

    }
}
