using System.ComponentModel.DataAnnotations;

namespace EXE.DataAccess
{
    public class ProjectOrder
    {
        [Key]
        public int ProjectOrderID { get; set; }

        public int? ProjectID { get; set; }

        public int? OrderID { get; set; }

        public virtual Project Project { get; set; }

        public virtual Order Order { get; set; }
    }
}
