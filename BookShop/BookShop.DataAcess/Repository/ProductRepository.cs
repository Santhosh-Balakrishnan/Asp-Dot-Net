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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext applicationDbContext) :base(applicationDbContext)
        {

        }
        public void Update(Product product)
        {
            var productFromDb = GetFirstOrDefault(p => p.Id == product.Id);
            if(productFromDb!=null)
            {
                productFromDb.Name = product.Name;
                productFromDb.ISBN = product.ISBN;
                productFromDb.Author = product.Author;
                productFromDb.Description = product.Description;
                productFromDb.Price = product.Price;
                productFromDb.Price50 = product.Price50;
                productFromDb.Price100 = product.Price100;
                productFromDb.ListPrice = product.ListPrice;
                productFromDb.CategoryId = product.CategoryId;
                productFromDb.CoverTypeId = product.CoverTypeId;
                productFromDb.ImageUrl = product.ImageUrl;
            }
        }
    }
}
