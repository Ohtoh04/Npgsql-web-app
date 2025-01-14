using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectDbWebApp.Domain {
    public class Unit {
        public int UnitId { get; set; } 
        public int ModuleId { get; set; } 
        public string Title { get; set; } 
        public string Description { get; set; } 
        public int? Sequence { get; set; }
        public CourseModule Module { get; set; }
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    }

}
