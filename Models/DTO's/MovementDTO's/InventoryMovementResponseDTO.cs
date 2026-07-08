using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.MovementDTO_s
{
    public class InventoryMovementResponseDTO
    {
        public int ID { get; set; }
        public int ProductId { get; set; }
        public required string ProductName { get; set; } 
        public required string ProductSku { get; set; } 
        public int Quantity { get; set; }
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public MovementType Movement { get; set; }
        public int UserID { get; set; }
        public required string UserName { get; set; } 
        public required string Reason { get; set; } 
        public DateTime Created { get; set; }
    }
}
