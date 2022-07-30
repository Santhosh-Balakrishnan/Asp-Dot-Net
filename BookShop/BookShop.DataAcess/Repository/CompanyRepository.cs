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
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext applicationDbContext) :base(applicationDbContext)
        {

        }
        public void Update(Company company)
        {
            dbContext.Companies.Update(company);
        }
    }
}
