using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace quest_web.Models
{
    [Table("shops")]
    public class Shop
    {
        public Shop()
        {
            this.Role = UserRole.ROLE_SHOP;
            this.IsPublished = false;
        }

        public Shop(string qabis, string password, UserRole role = UserRole.ROLE_SHOP)
        {
            this.Qabis = qabis;
            this.Password = password;
            this.IsPublished = false;
        }

        public Shop(String name, string qabis, string email, string password, UserRole role = UserRole.ROLE_SHOP)
        {
            this.Name = name;
            this.Qabis = qabis;
            this.Password = password;
            this.Role = role;
            this.Email = email;
            this.IsPublished = false;
        }

        [Column("id")]
        [Key]
        [Required]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("qabis")]
        [Required]
        public string Qabis { get; set; }

        [Column("password")]
        [Required]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Column("is_published")]
        public bool IsPublished { get; set; }

        [Column("role", TypeName = "nvarchar(255)")]
        public UserRole Role { get; set; }

        [Column("creation_date")]
        [DataType(DataType.DateTime)]
        public DateTime? CreationDate { get; set; }

        [Column("updated_date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        public List<ShopAddress> ShopAddresses { get; set; }

        public ICollection<HandiHelper> HandiHelpers { get; set; }

        public ICollection<Mark> Marks { get; set; }

        public ICollection<Review> Reviews { get; set; }

        public ICollection<Commentary> Commentaries { get; set; }

         public override bool Equals(Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Shop s = (Shop)obj;
                return (Qabis == s.Qabis);
            }
        }

        public override int GetHashCode()
        {
            return this.Qabis.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("Shop ({0}, {1}, {2}, {3}, {4}, {5})", Id,Qabis,Role,CreationDate,UpdatedDate);
        }
    }
}
