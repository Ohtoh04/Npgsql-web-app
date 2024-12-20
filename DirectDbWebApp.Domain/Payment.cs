using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectDbWebApp.Domain {
    public class Payment {
        public int PaymentId { get; set; }

        public int UserId { get; set; }

        public int CourseId { get; set; }

        public decimal Amount { get; set; }

        public DateTime DatePaid { get; set; } = DateTime.Now;

        public string? PaymentMethod { get; set; }

        public virtual DbUser? User { get; set; }


        public virtual Course? Course { get; set; }
    }
}
