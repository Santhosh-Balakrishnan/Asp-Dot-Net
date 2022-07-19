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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext applicationDbContext):base(applicationDbContext)
        {
        }

        public void Update(Category category)
        {
            dbContext.Categories.Update(category);
        }
    }
}
