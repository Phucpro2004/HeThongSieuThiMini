using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using MiniShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniShop.Services
{
    public class ProductService : IProductService
    {
        private readonly MiniShopDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(MiniShopDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResponse<ProductResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchKeyword = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                query = query.Where(p => p.Name.Contains(searchKeyword));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = _mapper.Map<IEnumerable<ProductResponse>>(products);

            return new PagedResponse<ProductResponse>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        public async Task<ProductResponse?> GetByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;
            return _mapper.Map<ProductResponse>(product);
        }

        public async Task<ProductResponse> CreateAsync(ProductCreateRequest request)
        {
            var product = _mapper.Map<Product>(request);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductResponse>(product);
        }

        public async Task<bool> UpdateAsync(int id, ProductCreateRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _mapper.Map(request, product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestockAsync(int productId, int quantity)
        {
            if (quantity <= 0) return false;
            
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.StockQuantity += quantity;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
