using MiniShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniShop.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> SearchByPhoneAsync(string phone);
    }
}
