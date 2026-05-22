using AutoMapper;
using BasErpFramework.Application.Dto;
using BasErpFramework.Application.Interface;
using BasErpFramework.Domain.Interface;
using BasErpFramework.Transversal.Common;
using BasErpFramework.Transversal.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasErpFramework.Application.Main
{
    public class AuthApplication : IAuthApplication
    {
        private readonly IUserDomain _userDomain;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly IAppLogger<AuthApplication> _logger;

        public AuthApplication(IUserDomain userDomain, IJwtService jwtService, IMapper mapper,IAppLogger<AuthApplication> logger)
        {
            _userDomain = userDomain;
            _jwtService = jwtService;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<Response<TokenDto>> SignInAsync(SignInDto signInDto)
        {
            var response = new Response<TokenDto>();
            try
            {
                var user = await _userDomain.GetByEmailAsync(signInDto.Email);
                if (user == null)
                {
                    
                    response.Message = "Usuario no encontrado";
                    _logger.LogError("fallo", response.Message);
                    return response;
                }
                var isvalidPassword = await _userDomain.CheckPasswordAsync(user, signInDto.Password);
                if (!isvalidPassword) {
                response.Message = "Contraseña incorrecta";
                    return response;
                }
                var token= _jwtService.GenerateToken(user);
                response.Data = new TokenDto
                {
                    Token = token,
                    TokenType = "Bearer",
                    Expiration = 3600
                };

                response.IsSuccess= true;
                response.Message = "Inicio de sesión exitoso";
            }
            catch (Exception ex)
            {

                response.Message = $"Error al iniciar sesión: {ex.Message}";
            }
            return response;
        }

        public async Task<Response<bool>> SignUpAsync(SignUpDto signUpDto)
        {
            var response = new Response<bool>();
            try
            {
                var existingUser = await _userDomain.GetByEmailAsync(signUpDto.Email);
                if(existingUser != null) 
                    {     
                        response.Message = "El usuario ya existe";
                        return response;
                    }
                var user = _mapper.Map<Domain.Entity.User>(signUpDto);
                response.Data = await _userDomain.CreateUserAsync(user, signUpDto.Password);

                if(response.Data)
                {
                    response.Message = "Usuario creado exitosamente";
                    response.IsSuccess = true;
                }
            }
            catch (Exception e)
            {

                response.Message = $"Error al crear el usuario: {e.Message}";

            }
            return response;
        }
    }
}
