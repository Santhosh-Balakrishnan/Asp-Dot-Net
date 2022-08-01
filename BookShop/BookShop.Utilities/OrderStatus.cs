using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Utilities
{
    public enum OrderStatus
    {
        Pending,
        Approved,
        Processing,
        Shipped ,
        Cancelled,
        Refunded
    }
}
