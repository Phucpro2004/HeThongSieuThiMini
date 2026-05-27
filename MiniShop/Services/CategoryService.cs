using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using MiniShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniShop.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly MiniShopDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(MiniShopDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
        }

        public async Task<CategoryResponse?> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;
            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryResponse> CreateAsync(CategoryCreateRequest request)
        {
            var category = _mapper.Map<Category>(request);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<bool> UpdateAsync(int id, CategoryCreateRequest request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _mapper.Map(request, category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
