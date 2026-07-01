using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.MovementDTO_s
{
    public class CreateInventoryMovementRequestDTO
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [EnumDataType(typeof(MovementType))]
        public MovementType Movement { get; set; }

        [Range(1, int.MaxValue)]
        public int UserID { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
