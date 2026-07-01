using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.MovementDTO_s
{
    public class InventoryMovementResponseDTO
    {
        public int ID { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public MovementType Movement { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}
