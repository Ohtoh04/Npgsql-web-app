using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectDbWebApp.Domain {
    public class DbUser {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public string Role { get; set; } = "Student";
        public string? ProfilePicture {get; set; }
        public DateTime DateRegistered { get; set; }

    }
}
