using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusParkManagementSystem
{
    public class Permission
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Module { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}