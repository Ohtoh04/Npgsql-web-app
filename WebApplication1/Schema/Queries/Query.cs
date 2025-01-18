using Courses.Api.Schema.Types;
using DirectDbWebApp.Domain;

namespace Courses.Api.Schema.Queries {
    public class Query {
        public CourseType GetCourse() =>
            new CourseType {
                Title = "C# in depth.",
                Description = "aboba",
            };
    }

}
