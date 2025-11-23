using System;

namespace ProductService.Mapper;

using AutoMapper;
using ProductService.Dtos;
using ProductService.Model;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Source → Destination
        CreateMap<ProductCreateDto, Product>();
    }
}
