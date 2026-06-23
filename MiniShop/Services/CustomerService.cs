using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.Interfaces;
using MiniShop.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniShop.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly MiniShopDbContext _context;

        public CustomerService(MiniShopDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                                 .OrderByDescending(c => c.CreatedAt)
                                 .ToListAsync();
        }

        public async Task<Customer?> SearchByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;
                
            return await _context.Customers
                                 .FirstOrDefaultAsync(c => c.Phone == phone);
        }
    }
}
