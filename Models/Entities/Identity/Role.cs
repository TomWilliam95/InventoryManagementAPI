namespace InventoryManagementAPI.Models.CoreModels.RolePermissions
{
    public class Role
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        [Timestamp]
        public byte[] RowVersion { get; internal set; } = [];

        public ICollection<UserRole> UserRoles { get; set; } = [];
        public ICollection<RolePermission> RolePermissions { get; set; } = [];
    }
}
