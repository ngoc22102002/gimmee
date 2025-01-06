using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE.DataAccess
{
    public partial class User
    {
        [Key]
        public int UserID { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public string? Avatar { get; set; }

        public string? Role { get; set; }

        public string? Gmail { get; set; }

        public string? Address { get; set; }

        public virtual ICollection<Project> Projects { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
