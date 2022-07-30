using BookShop.DataAcess.Repository.IRepository;
using BookShopWeb.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookShop.DataAcess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDbContext dbContext;
        protected DbSet<T> dbSet;

        public Repository(ApplicationDbContext _dbContext)
        {
            dbContext = _dbContext;
            dbSet = dbContext.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? expression = null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (expression != null)
                query = dbSet.Where(expression);
            if (!string.IsNullOrEmpty(includeProperties))
                query = AddForeignkeyProperties(includeProperties, query);
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> expression, string? includeProperties = null)
        {
            IQueryable<T> query=dbSet.Where(expression);
            if (!string.IsNullOrEmpty(includeProperties))
                query = AddForeignkeyProperties(includeProperties, query);
            return query.FirstOrDefault<T>();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        private IQueryable<T> AddForeignkeyProperties(string includeProperties, IQueryable<T> query)
        {
            if (includeProperties != null)
            {
                foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = prop.Aggregate(query, (current, include) => current.Include(prop));
                }
            }
            return query;
        }
    }
}
