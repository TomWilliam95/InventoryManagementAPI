using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InventoryManagementAPI.Models.CoreModels.UserModels
{
    public class User
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public required string Email { get; set; }

        [Required]
        [StringLength(255)]
        public required string Password_Hash { get; set; }

        public DateTime LastLogin { get; set; }
        public DateTime Updated { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;

        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

        public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

        public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    }
}
