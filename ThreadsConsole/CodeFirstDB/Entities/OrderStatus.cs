using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstDB.Entities
{
    [Table("tblOrderStatuses")]
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(300)]
        public string Name { get; set; }
    }
}
