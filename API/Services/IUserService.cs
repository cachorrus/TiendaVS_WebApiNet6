using API.Dtos;

namespace API.Services;

public interface IUserService
{
    Task<string> RegisterAsync(RegisterDto registerDto);
    Task<DatosUsuarioDto> GetTokenAsync(LoginDto loginDto);
    Task<string> AddRoleAsync(AddRoleDto addRoleDto);
    Task<DatosUsuarioDto> RefreshTokenAsync(string refreshToken);
}
