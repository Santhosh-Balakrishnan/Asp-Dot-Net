using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using BookShopWeb.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAcess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        public OrderHeaderRepository(ApplicationDbContext applicationDbContext):base(applicationDbContext)
        {
        }

        //public override IEnumerable<OrderHeader> GetAll(Expression<Func<OrderHeader, bool>>? expression = null, string? includeProperties = null)
        //{
        //    IQueryable<OrderHeader> query = dbSet;

        //    if (expression != null)
        //        query = dbSet.Where(expression);
        //    if (!string.IsNullOrEmpty(includeProperties))
        //        query = AddForeignkeyProperties(includeProperties, query);
        //    return query.ToList();
        //}

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

        public void UpdatePaymentStatus(int id, string sessionId, string paymentIntentId)
        {
            var orderHeader = dbContext.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if (orderHeader != null)
            {
                orderHeader.SessionId = sessionId;
                orderHeader.PaymentIntentId = paymentIntentId;
            }
        }

        private IQueryable<OrderHeader> AddForeignkeyProperties(string includeProperties, IQueryable<OrderHeader> query)
        {
            if (includeProperties != null)
            {
                foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query.Include(prop);
                }
            }
            return query;
        }
    }
}
