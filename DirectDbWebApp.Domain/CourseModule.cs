using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectDbWebApp.Domain {
    public class CourseModule {
        public int ModuleId { get; set; } 
        public int CourseId { get; set; } 
        public string Title { get; set; } 
        public string Description { get; set; } 
        public int? Sequence { get; set; } 
        public Course Course { get; set; }
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
    }

}
