using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace quest_web.Models
{
    [Table("marks")]
    public class Mark
    {
        public Mark()
        {
        }

        public Mark(int score, User user, Shop shop)
        {
            this.Score = score;
            this.user = user;
            this.shop = shop;
        }

        [Column("id")]
        [Key]
        [Required]
        public int Id { get; set; }

        [Column("score")]
        [Required]
        public int Score { get; set; }

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
