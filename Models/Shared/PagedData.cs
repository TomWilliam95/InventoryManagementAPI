namespace InventoryManagementAPI.Models.Shared
{
    public class PagedData<T>
    {
        public required IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
    }
}

