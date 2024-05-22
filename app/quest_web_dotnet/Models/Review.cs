using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace quest_web.Models
{
    [Table("reviews")]
    public class Review
    {
        public Review()
        {
        }

        public Review(String comments, User user, Shop shop)
        {
            this.Comments = comments;
            this.IsApproved = false;
            this.user = user;
            this.shop = shop;
        }

        [Column("id")]
        [Key]
        [Required]
        public int Id { get; set; }

        [Column("comments")]
        [Required]
        public string Comments { get; set; }

        [Column("is_approved")]
        [Required]
        public bool IsApproved { get; set; }

        [Column("user_id")]
        [Required]
        public User user { get; set; }

        [Column("shop_id")]
        [Required]
        public Shop shop { get; set; }

        [Column("creation_date")]
        [DataType(DataType.DateTime)]
        public DateTime? CreationDate { get; set; }

        [Column("updated_date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }
    }
}
