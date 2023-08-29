using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.ENTITIES.Models
{
    public class Order : BaseEntity
    {
        public string ShippedAddress { get; set; }
        public int? AppUserID { get; set; }
        public int? ShipperID { get; set; }

        //these properties opened to access order process faster. this type of use it not really optimal, should not be abused.
        public decimal TotalPrice { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        //Relational Properties

        public virtual AppUser AppUser { get; set; }
        public virtual List<OrderDetail> OrderDetails { get; set; }
        public virtual Shipper Shipper { get; set; }
    }
}
