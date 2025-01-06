using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE.DataAccess
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public int? UserID { get; set; }

        public int? ProjectID { get; set; }

        public double? Total { get; set; }

        public string? Status { get; set; }

        public string? Note { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<ProjectOrder> ProjectOrders { get; set; }
    }
}
