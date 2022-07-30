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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(ApplicationDbContext applicationDbContext):base(applicationDbContext)
        {
        }

        public void Update(OrderDetail orderDetail)
        {
            dbContext.OrderDetails.Update(orderDetail);
        }
    }
}
