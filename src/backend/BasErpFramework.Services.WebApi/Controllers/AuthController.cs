using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BasErpFramework.Application.Dto;
using BasErpFramework.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace BasErpFramework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Operaciones relacionadas con Autenticación")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthApplication _authApplication;
        public AuthController(IAuthApplication authApplication)
        {
            _authApplication = authApplication;
        }

        [HttpPost("SignUp")]
        [SwaggerOperation(
        Summary = "Registra un nuevo Usuario")]
        public async Task<IActionResult> SignUpAsync([FromBody] SignUpDto signupDto)
        {
            var response = await _authApplication.SignUpAsync(signupDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [AllowAnonymous]
        [HttpPost("SignIn")]
        [SwaggerOperation(Summary = "Autentica un usuario y genera token")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInDto signInDto)
        {
            var response = await _authApplication.SignInAsync(signInDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return Unauthorized(response);
        }
    }
}
