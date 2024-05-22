using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace quest_web.Models
{
    [Table("commentaries")]
    public class Commentary
    {
        public Commentary()
        {
        }

        public Commentary(String comment, User user, Shop shop)
        {
            this.Comment = comment;
            this.user = user;
            this.shop = shop;
        }

        [Column("id")]
        [Key]
        [Required]
        public int Id { get; set; }

        [Column("comment")]
        [Required]
        public string Comment { get; set; }

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
