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
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        public CoverTypeRepository(ApplicationDbContext applicationDbContext) :base(applicationDbContext)
        {

        }
        public void Update(CoverType coverType)
        {
            dbContext.CoverTypes.Update(coverType);
        }
    }
}
