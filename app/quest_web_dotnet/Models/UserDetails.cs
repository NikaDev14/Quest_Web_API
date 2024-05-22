using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace quest_web.Models
{
    public class UserDetails
    {
        public UserDetails()
        {
        }

        public UserDetails(string username, UserRole role)
        {
            this.Username = username;
            this.Role = role;
        }

        public string Username { get; set; }

        public UserRole Role { get; set; }
    }
}
