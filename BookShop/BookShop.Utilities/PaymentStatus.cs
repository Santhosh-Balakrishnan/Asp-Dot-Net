using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Utilities
{
    public enum PaymentStatus
    {
        Pending,
        Approved,
        ApprovedForDelayedPayment,
        Rejected,
        Refunded
    }
}
