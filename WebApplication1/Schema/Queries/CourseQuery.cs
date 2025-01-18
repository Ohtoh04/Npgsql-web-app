using Courses.Api.Services.CourseApiService;
using DirectDbWebApp.Domain;

namespace Courses.Api.Schema.Queries {
    public class CourseQuery {
        private readonly ICourseApiService _courseApiService;

        public CourseQuery(ICourseApiService courseApiService) {
            _courseApiService = courseApiService;
        }


        public async Task<IEnumerable<Course>> GetCoursesAsync() =>
            await _courseApiService.GetCoursesAsync();


        public async Task<Course?> GetCourseByIdAsync(int id) =>
            await _courseApiService.GetCourseByIdAsync(id);


        public async Task<IEnumerable<Course>> GetCoursesFilteredAsync(string? title = null, double maxPrice = 100000, double minRating = 0,
                                                                       string? courseType = null, string? category = null) =>
            await _courseApiService.GetCoursesFilteredAsync(title, maxPrice, minRating, courseType, category);
        

        public async Task<IEnumerable<Course>> GetUsersCoursesAsync(string userId) =>
            await _courseApiService.GetUsersCoursesAsync(userId);
    }
}
