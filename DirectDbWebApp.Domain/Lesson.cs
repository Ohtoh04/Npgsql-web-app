using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DirectDbWebApp.Domain {
    public class Lesson {
        public int LessonId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int Sequence { get; set; }
        public int UnitId { get; set; }
        public JsonElement CorrectSolution { get; set; }
    }
}
