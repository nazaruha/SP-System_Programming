using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstDB.Entities
{
    [Table("tblOrders")]
    public class OrderEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
        [ForeignKey("Status")]
        public int StatusId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual UserEntity User { get; set; }
        public virtual OrderStatus Status { get; set; }
    }
}
