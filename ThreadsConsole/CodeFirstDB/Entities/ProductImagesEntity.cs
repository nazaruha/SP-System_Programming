using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstDB.Entities
{
    [Table("tblProductImages")]
    public class ProductImagesEntity
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(200)]
        public string Name { get; set; }
        [Required]
        public int Priority { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public virtual ProductEntity Product { get; set; }
    }
}
