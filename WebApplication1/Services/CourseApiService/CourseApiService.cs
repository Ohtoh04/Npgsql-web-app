using Dapper;
using DirectDbWebApp.Domain;
using Npgsql;

namespace Courses.Api.Services.CourseApiService {
    public class CourseApiService : ICourseApiService {
        private readonly string _connectionString;
        public CourseApiService(IConfiguration configuration) {
            this._connectionString = configuration.GetValue<string>("ConnectionString") ?? "";
        }

        public async Task<int> CreateCourse(Course course) {
            var insertCourseQuery = @"INSERT INTO course (title, description, coursetype, price, duration, rating)
                                      VALUES (@Title, @Description, @CourseType, @Price, @Duration, @Rating)
                                      RETURNING course_id;";

            int createdCourseId = 0;

            using (var connection = new NpgsqlConnection(this._connectionString)) {
                var parameters = new { Title = course.Title, Description = course.Description, 
                    CourseType = course.CourseType, Price = course.Price, Duration = course.Duration,
                    Rating = course.Rating };
                createdCourseId = (int?)await connection.ExecuteScalarAsync(insertCourseQuery, parameters) ?? 0;
            }

            return createdCourseId;
        }

        public async Task DeleteCourse(int id) {
            var query = "DELETE FROM course WHERE course_id = @Id";
            var parameters = new { Id = id };

            await using var connection = new NpgsqlConnection(this._connectionString);
            await connection.ExecuteAsync(query, parameters);
        }

        public async Task<Course> GetCourseByIdAsync(int id) {
            var query = "SELECT * FROM Course WHERE course_id = @id";
            var parameters = new { Id = id };


            await using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QuerySingleAsync<Course>(query, parameters);    
        }

        public async Task<IEnumerable<Course>> GetCoursesAsync() {
            var query = @"SELECT c.course_id, c.title, c.rating, cat.name AS category
                          FROM Course c
                          LEFT JOIN CourseCategory cc ON c.course_id = cc.course_id
                          LEFT JOIN Category cat ON cc.category_id = cat.category_id";
            var courses = new List<object>();

            await using var connection = new NpgsqlConnection(_connectionString);

            return await connection.QueryAsync<Course>(query);
        }

        public async Task<IEnumerable<Course>> GetCoursesFilteredAsync(string title = "", double maxPrice = 100000, double minRating = 0, string courseType = "", string category = "") {
            // Start with the base query
            var query = @"SELECT *,cat.name AS category
                          FROM Course c
              	          LEFT JOIN CourseCategory cc ON c.course_id = cc.course_id
              	          JOIN Category cat ON cc.category_id = cat.category_id
              	          WHERE price < @MaxPrice AND rating > @MinRating";

            await using var connection = new NpgsqlConnection(_connectionString);

            var parameters = new Dictionary<string, object> {
                { "@MaxPrice", maxPrice },
                { "@MinRating", minRating/10.0 }
            };

            // Append filters dynamically
            if (!string.IsNullOrWhiteSpace(title)) {
                query += " AND title ILIKE @Title";
                parameters.Add("@Title", $"%{title}%");
            }

            if (!string.IsNullOrWhiteSpace(courseType)) {
                query += " AND coursetype = @CourseType";
                parameters.Add("@CourseType", courseType);
            }

            if (!string.IsNullOrWhiteSpace(category)) {
                query += " AND cat.name = @Category";
                parameters.Add("@Category", category);
            }
           
            return  await connection.QueryAsync<Course>(query, parameters);
        }

        public async Task<IEnumerable<Course>> GetUsersCoursesAsync(string userId) {
            var query = @"SELECT c.course_id, c.title, c.rating, c.coursetype, c.price, c.duration, c.date_created, cu.relation_type
              	        FROM Course c
              	        JOIN CourseUser cu ON c.course_id = cu.course_id
              	        WHERE cu.user_id = @UserId";

            await using var connection = new NpgsqlConnection(_connectionString);

            var parameters = new { UserId = userId };

            return await connection.QueryAsync<Course>(query, parameters);
        }

        public async Task UpdateCourse(Course course) {
            var query = @"UPDATE Course
                          SET title = @Title,
                          description = @Description,
                          coursetype = @CourseType,
                          price = @Price,
                          duration = @Duration,
                          rating = @Rating
                          WHERE course_id = @Id";

            var parameters = new {
                Title = course.Title, Description = course.Description, CourseType = course.CourseType,
                Price = course.Price, Duration = course.Duration, Rating = course.Rating, Id = course.CourseId
            };

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.QueryAsync(query, parameters);
        }
    }
}
