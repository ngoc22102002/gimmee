using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE.DataAccess
{
    public class Material
    {
        [Key]
        public int MaterialID { get; set; }

        public string? PaperName { get; set; }

        public string? SpringName { get; set; }

        public int? NumberOfPage { get; set; }

        public string? Size { get; set; }

        public double? Price { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
    }
}
