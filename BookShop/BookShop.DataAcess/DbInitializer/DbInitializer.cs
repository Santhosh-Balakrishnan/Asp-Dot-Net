using BookShop.DataAcess.Repository.IRepository;
using BookShop.Models;
using BookShop.Utilities;
using BookShopWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAcess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public void Initialize()
        {
            try
            {
                if(_dbContext.Database.GetPendingMigrations().Count()>0)
                {
                    _dbContext.Database.Migrate();
                }
            }catch
            {

            }
            //Add Migrations
            //create roles
            if (!_roleManager.RoleExistsAsync(Role.Admin.ToString()).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Role.Admin.ToString())).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Role.Individual.ToString())).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Role.Company.ToString())).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Role.Employee.ToString())).GetAwaiter().GetResult();

                //If roles created create admin user
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    Name = "admin",
                    PhoneNumber = "1234567890",
                },"Admin@123").GetAwaiter().GetResult();
                ApplicationUser user = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
                if(user!=null)
                    _userManager.AddToRoleAsync(user, Role.Admin.ToString()).GetAwaiter().GetResult();
            }
            return;
            
        }
    }
}
