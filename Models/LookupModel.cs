using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusParkManagementSystem.Models
{
    /// <summary>
    /// Базовый класс для всех справочников
    /// </summary>
    public abstract class LookupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}