using InventoryManagementAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class User
    {
        [Key]
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password_Hash { get; set; }
        public UserRoles Role { get; set; }
        public string ApiKey { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateOnly Created { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<InventoryMovement>? InventoryMovements { get; set; }
    }
}