using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectDbWebApp.Domain {
    public class Course {
        public int CourseId { get; set; } // Maps to course_id
        public string Title { get; set; } // Maps to title
        public string Description { get; set; } // Maps to description
        public string CourseType { get; set; } // Maps to coursetype
        public decimal? Price { get; set; } // Maps to price
        public TimeSpan? Duration { get; set; } // Maps to duration
        public DateTime DateCreated { get; set; } // Maps to date_created
        public decimal? Rating { get; set; } // Maps to rating

        // Navigation property for related Modules
        public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
    }

}
