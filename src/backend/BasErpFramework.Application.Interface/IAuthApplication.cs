using BasErpFramework.Application.Dto;
using BasErpFramework.Transversal.Common;

namespace BasErpFramework.Application.Interface
{
    public interface IAuthApplication
    {
        Task<Response<bool>>SignUpAsync(SignUpDto signUpDto);
        Task<Response<TokenDto>> SignInAsync(SignInDto signInDto);    
    }
}
