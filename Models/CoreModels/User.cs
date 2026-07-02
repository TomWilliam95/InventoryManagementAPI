using InventoryManagementAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class User
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password_Hash { get; set; }

        [EnumDataType(typeof(UserRoles))]
        public UserRoles Role { get; set; }

        public DateTime LastLogin { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateOnly Created { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<InventoryMovement>? InventoryMovements { get; set; }
    }
}
