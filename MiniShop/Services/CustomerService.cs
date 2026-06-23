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

        public async Task<Customer> CreateAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer?> UpdateAsync(int id, Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null) return null;

            existingCustomer.FullName = customer.FullName;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Email = customer.Email;
            existingCustomer.Address = customer.Address;
            // Note: points are usually updated via a separate transaction/logic, but we allow editing here if needed, or keep it as is.
            existingCustomer.Points = customer.Points;

            await _context.SaveChangesAsync();
            return existingCustomer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
