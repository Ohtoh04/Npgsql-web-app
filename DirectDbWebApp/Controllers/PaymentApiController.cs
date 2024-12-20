using DirectDbWebApp.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DirectDbWebApp.Controllers {
    public class PaymentApiController : ControllerBase {
        private readonly string _connectionString;
        public PaymentApiController(IConfiguration configuration) {
            this._connectionString = configuration.GetValue<string>("ConnectionString") ?? "";
        }

        [HttpPost]
        [Route("api/payment/create")]
        public IActionResult CreatePayment([FromForm] Payment payment) {
            if (payment == null) {
                return BadRequest("Invalid payment data.");
            }

            try {
                using (var connection = new NpgsqlConnection(_connectionString)) {
                    connection.Open();

                    string query = @"INSERT INTO Payment (user_id, course_id, amount, date_paid, payment_method) 
                                 VALUES (@UserId, @CourseId, @Amount, @DatePaid, @PaymentMethod) 
                                 RETURNING payment_id;";

                    using (var command = new NpgsqlCommand(query, connection)) {
                        // Add parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@UserId", payment.UserId);
                        command.Parameters.AddWithValue("@CourseId", payment.CourseId);
                        command.Parameters.AddWithValue("@Amount", payment.Amount);
                        command.Parameters.AddWithValue("@DatePaid", payment.DatePaid);
                        command.Parameters.AddWithValue("@PaymentMethod", (object)payment.PaymentMethod ?? DBNull.Value);

                        // Execute the query and retrieve the generated payment_id
                        var paymentId = command.ExecuteScalar();

                        return Ok(new { Message = "Payment created successfully.", PaymentId = paymentId });
                    }
                }
            } catch (Exception ex) {
                // Log the exception (if logging is set up)
                Console.WriteLine(ex);
                return StatusCode(500, "An error occurred while creating the payment.");
            }
        }
    }
}
