using AutoMapper;
using BasErpFramework.Application.Dto;
using BasErpFramework.Domain.Entity;    

namespace BasErpFramework.Application.Main.Commons
{
    public class MappingsProfile:Profile
    {
        public MappingsProfile() { 
          CreateMap<Producto, ProductoDto>().ReverseMap();
          CreateMap<User,SignUpDto>().ReverseMap();

        }
    }
}
