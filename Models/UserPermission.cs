namespace BusParkManagementSystem
{
    public class UserPermission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemCode { get; set; }
        public string MenuItemName { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}