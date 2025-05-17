using Auth_with_JWT.Data;
using Auth_with_JWT.Entities;
using Auth_with_JWT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth_with_JWT.Services
{
	public class AuthService(IConfiguration configuration, MyDbContext context) : IAuthService
	{
		private readonly IConfiguration _configuration = configuration;
		private readonly MyDbContext context = context;

		public async Task<User> RegisterAsync(UserDTO request)
		{
			if (await context.Users.AnyAsync(u => u.Username == request.Username))
				return null;
			var user = new User();

			user.Username = request.Username;
			user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
			await context.Users.AddAsync(user);
			await context.SaveChangesAsync();
			return user;
		}

		public async Task<TokenResponseDTO> LoginAsync(UserDTO request)
		{
			User? user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

			if (user is null)
				return null;

			if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
== PasswordVerificationResult.Failed)
			{
				return null;
			}
			var token = new TokenResponseDTO
			{
				AccessToken = CreateToken(user),
				RefreshToken = await GenerateAndSaveRefreshToken(user)
			};

			return token;
		}

		public async Task<string> GenerateAndSaveRefreshToken(User user)
		{
			var randomnumber = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomnumber);
			var refreshToken = Convert.ToBase64String(randomnumber);
			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(1);
			await context.SaveChangesAsync();
			return refreshToken;

		}

		private string CreateToken(User user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Role, user.Role ?? string.Empty), // Ensure Role is not null
            };

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
				_configuration.GetValue<string>("AppSettings:Token")!
			));

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			var tokenDistributer = new JwtSecurityToken(
				issuer: _configuration["AppSettings:Issuer"],
				audience: _configuration["AppSettings:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddDays(1),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(tokenDistributer);
		}

		public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request)
		{
			var user = await context.Users.FindAsync(request.UserId);
			if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
				return null;
			var token = new TokenResponseDTO
			{
				AccessToken = CreateToken(user),
				RefreshToken = await GenerateAndSaveRefreshToken(user)
			};
			return token;
		}
	}
}

