using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE.DataAccess
{
    public partial class Project
    {
        [Key]
        public int ProjectID { get; set; }

        public int? UserID { get; set; }

        public int? MaterialID { get; set; }

        public string? ImageFront { get; set; }

        public string? ImageBack { get; set; }

        public string? Note { get; set; }
        
        public string? BookName { get; set; }

        public virtual User User { get; set; }

        public virtual Material Material { get; set; }

        public virtual ICollection<ProjectOrder> ProjectOrders { get; set; }
    }
}
