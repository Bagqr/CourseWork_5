using System;

namespace BusParkManagementSystem
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int? ParentId { get; set; }
        public int DisplayOrder { get; set; }
    }
}