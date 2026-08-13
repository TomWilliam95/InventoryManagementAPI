using InventoryManagementAPI.Models.CoreModels.MovementModels;

namespace InventoryManagementAPI.Models.CoreModels.UserModels
{
    public class Customer
    {
        [Key]
        public int ID { get; set; }

        public int? UserID { get; set; }
        public User? User { get; set; }

        [StringLength(100)]
        public required string FirstName { get; set; }

        [StringLength(100)]
        public required string LastName { get; set; }

        [Phone]
        [StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? BillingAddress { get; set; }

        [StringLength(500)]
        public string? ShippingAddress { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsActive { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; internal set; } = [];

        public ICollection<SalesOrder> SalesOrders { get; set; } = [];
    }
}
