using System.Dynamic;


namespace DirectDbWebApp.ViewModels {
    public class CourseTreeViewModel {
        public CourseTreeViewModel(dynamic Course, List<dynamic> Modules, List<List<dynamic>> Units) { 
            this.Course = Course;
            this.Modules = Modules;
            this.Units = Units;
        }
        public dynamic? Course { get; set; }
        public List<dynamic> Modules { get; set; }
        public List<List<dynamic>> Units { get; set; }

    }
}
