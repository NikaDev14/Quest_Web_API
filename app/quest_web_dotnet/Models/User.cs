using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace quest_web.Models
{
    [Table("user")]
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {

        public User()
        {
        }

        public User(string username, string password,  UserRole role=UserRole.ROLE_USER)
        {
            Username = username;
            Password = password;
            Role = role;
        }
        [Column("id")]
        [Key]
        [Required]
        public int Id { get; set; }


        [Column("username")]
        [Required]
        [StringLength(255)]
        public string Username { get; set; }

        [Column("password")]
        [Required]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Column("role",TypeName = "nvarchar(255)")]
        public UserRole Role { get; set; }

        [Column("creation_date")]
        [DataType(DataType.DateTime)]
        public DateTime? CreationDate { get; set; }

        [Column("updated_date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        public List<Address> Addresses { get; set; }

        public ICollection<Mark> Marks { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Commentary> Commentaries { get; set; }
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                User u = (User)obj;
                return (Username == u.Username);
            }
        }

        public override int GetHashCode()
        {
            return this.Username.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("User ({0}, {1}, {2}, {3}, {4}, {5})", Id,Username,Role,CreationDate,UpdatedDate);
        }

    }
}
