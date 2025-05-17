using Auth_with_JWT.Entities;
using Auth_with_JWT.Models;

namespace Auth_with_JWT.Services
{
	public interface IAuthService
	{
		Task<TokenResponseDTO?> LoginAsync(UserDTO request);
		Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request);
		Task<User?> RegisterAsync(UserDTO request);
	}
}