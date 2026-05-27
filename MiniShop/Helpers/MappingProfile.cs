using AutoMapper;
using MiniShop.DTOs;
using MiniShop.Models;

namespace MiniShop.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
            CreateMap<ProductCreateRequest, Product>();

            CreateMap<Category, CategoryResponse>();
            CreateMap<CategoryCreateRequest, Category>();
            
            CreateMap<Order, OrderResponse>();
            CreateMap<OrderDetail, OrderDetailResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
                
            CreateMap<User, UserResponse>();
        }
    }
}
