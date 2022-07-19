using BookShop.DataAcess.Repository.IRepository;
using BookShopWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAcess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext applicationDbContext;
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            applicationDbContext = dbContext;
            Category = new CategoryRepository(dbContext);
            CoverType = new CoverTypeRepository(dbContext);
            Product = new ProductRepository(dbContext);
        }
        public ICategoryRepository Category {get;private set;}
        public ICoverTypeRepository CoverType {get;private set;}
        public IProductRepository Product {get;private set;}

        public void Save()
        {
            applicationDbContext.SaveChanges();
        }
    }
}
