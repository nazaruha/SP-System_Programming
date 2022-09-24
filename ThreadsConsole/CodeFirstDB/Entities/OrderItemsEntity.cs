using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstDB.Entities
{
    [Table("tblOrederItems")]
    public class OrderItemsEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int PriceBuy { get; set; }
        [Required]
        public int Count { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual OrderEntity Order { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual ProductEntity Product { get; set; }

    }
}
