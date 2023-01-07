using API.Dtos;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class UsuariosController : BaseAPIController
    {
        private readonly IUserService _userService;

        public UsuariosController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterAsync(RegisterDto registerDto)
        {
            var result = await _userService.RegisterAsync(registerDto);

            return Ok(result);
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetTokenAsync(LoginDto loginDto)
        {
            var result = await _userService.GetTokenAsync(loginDto);
            SetRefreshTokenInCookie(result.RefreshToken);

            return Ok(result);
        }

        [HttpPost("addrole")]
        public async Task<IActionResult> AddRoleAsync(AddRoleDto addRoleDto)
        {
            var result = await _userService.AddRoleAsync(addRoleDto);

            return Ok(result);  
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(string refre)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var resultado = await _userService.RefreshTokenAsync(refreshToken);
            
            if(!String.IsNullOrEmpty(resultado.RefreshToken))
            {
                SetRefreshTokenInCookie(resultado.RefreshToken);
            }

            return Ok(resultado);
        }
        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(10),
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}
