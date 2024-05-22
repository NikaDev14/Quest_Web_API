using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace quest_web.Models
{
    [Table("handi_helpers")]
    public class HandiHelper
    {
        public HandiHelper()
        {
        }

        public HandiHelper(HandiHelperEnum label)
        {
            this.Label = label;
        }

        [Column("id")]
        [Key]
        [Required]
        public int Id { get; set; }

        [Column("label", TypeName = "nvarchar(255)")]
        public HandiHelperEnum Label { get; set; }

        public ICollection<Shop> Shops { get; set; }

        [Column("creation_date")]
        [DataType(DataType.DateTime)]
        public DateTime? CreationDate { get; set; }

        [Column("updated_date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }
    }
}
