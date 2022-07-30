using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using BookShopWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAcess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        public OrderHeaderRepository(ApplicationDbContext applicationDbContext):base(applicationDbContext)
        {
        }

        public void Update(OrderHeader orderHeader)
        {
            dbContext.OrderHeaders.Update(orderHeader);
        }
        public void UpdateOrderStatus(int id, string? orderStatus = null, string? paymentStatus = null)
        {
            var orderHeader=dbContext.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if(orderHeader!=null)
            {
                orderHeader.OrderStatus = orderStatus;
                if(paymentStatus!=null)
                    orderHeader.PaymentStatus = paymentStatus;
            }
        }
    }
}
