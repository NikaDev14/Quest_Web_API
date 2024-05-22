using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace quest_web.Models
{
    [Table("address")]
    public class Address
    {
        public Address()
        {

        }

        public Address(string road, string postalCode, string city, string country)
        {
            this.Road = road;
            this.PostalCode = postalCode;
            this.City = city;
            this.Country = country;
        }

        [Column("id")]
        [Key]
        [Required]
        public int Id { get; set; }

        [Column("street")]
        [StringLength(100)]
        [Required]
        public string Road { get; set; }


        [Column("postal_code")]
        [StringLength(30)]
        [Required]
        public string PostalCode { get; set; }

        [Column("city")]
        [StringLength(50)]
        [Required]
        public string City { get; set; }

        [Column("country")]
        [StringLength(50)]
        [Required]
        public string Country { get; set; }

        [Column("creation_date")]
        [DataType(DataType.DateTime)]
        public DateTime? CreationDate { get; set; }

        [Column("updated_date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        public User user { get; set; }
    }
}
