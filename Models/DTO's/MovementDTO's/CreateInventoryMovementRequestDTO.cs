using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.MovementDTO_s
{
    public class CreateInventoryMovementRequestDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public MovementType Movement { get; set; }
        public int UserID { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
