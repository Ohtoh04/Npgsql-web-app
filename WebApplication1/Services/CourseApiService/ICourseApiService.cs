using DirectDbWebApp.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Courses.Api.Services.CourseApiService {
    public interface ICourseApiService {
        /// <summary>
        /// Returns all courses
        /// </summary>
        public Task<IEnumerable<Course>> GetCoursesAsync();

        /// <summary>
        /// Returns courses of a user with a given userId
        /// </summary>
        public Task<IEnumerable<Course>> GetUsersCoursesAsync(string userId);

        /// <summary>
        /// Returns a course with a given id
        /// </summary>
        public Task<Course> GetCourseByIdAsync(int id);

        /// <summary>
        /// Returns a course list according to passed parameters
        /// </summary>
        public Task<IEnumerable<Course>> GetCoursesFilteredAsync(string Title = "", double MaxPrice = 100000,
                                         double MinRating = 0, string courseType = "", string category = "");

        /// <summary>
        /// Creates a new course and returns id of the created course or 0 if creation failed
        /// </summary>
        public Task<int> CreateCourse(Course course);

        /// <summary>
        /// Updates a caurse by id with the data of the passed object
        /// </summary>
        public Task UpdateCourse(Course course);

        /// <summary>
        /// Deletes a course by id
        /// </summary>
        public Task DeleteCourse(int id);
    }
}
