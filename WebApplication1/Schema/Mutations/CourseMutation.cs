using Courses.Api.Schema.Types;
using Courses.Api.Services.CourseApiService;
using DirectDbWebApp.Domain;

namespace Courses.Api.Schema.Mutations {
    public class CourseMutation {
        private readonly ICourseApiService _courseApiService;

        public CourseMutation(ICourseApiService courseApiService) {
            _courseApiService = courseApiService;
        }

        public async Task<int> CreateCourse(CourseType course) {
            var newCourse = new Course() {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                CourseType = course.CourseAvailabilityType,
                Price = course.Price,
                Duration = course.Duration,
                DateCreated = course.DateCreated,
                Rating = course.Rating,
            };
            return await _courseApiService.CreateCourse(newCourse);
        }

        public async Task DeleteCourse(int id) =>
            await _courseApiService.DeleteCourse(id);

        public async Task UpdateCourse(CourseType course) {
            var updatedCourse = new Course() {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                CourseType = course.CourseAvailabilityType,
                Price = course.Price,
                Duration = course.Duration,
                DateCreated = course.DateCreated,
                Rating = course.Rating,
            };

            await _courseApiService.UpdateCourse(updatedCourse);
        }
    }
}
