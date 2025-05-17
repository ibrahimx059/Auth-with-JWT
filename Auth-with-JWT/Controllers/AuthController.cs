using Auth_with_JWT.Entities;
using Auth_with_JWT.Models;
using Auth_with_JWT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth_with_JWT.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService service;

		public AuthController(IAuthService service)
		{
			this.service = service;
		}

		[HttpPost("register")]
		public async Task<ActionResult<User>> Register(UserDTO request)
		{
			var user = await service.RegisterAsync(request);
			if (user is null)
				return BadRequest("User Already Exist");
			return Ok(user);
		}

		[HttpPost("login")]
		public async Task<ActionResult<TokenResponseDTO>> Login(UserDTO request)
		{
			var token = await service.LoginAsync(request);
			if (token is null)
				return BadRequest("Wrong username or password");
			return Ok(token);
		}


		[HttpPost("refresh-token")]
		public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
		{
			var token = await service.RefreshTokenAsync(request);
			if (token is null)
				return BadRequest("Invalid/Expire Token");
			return Ok(token);
		}


		[HttpGet("Auth-endpoint")]
		[Authorize]
		public ActionResult AuthCheck()
		{
			return Ok();
		}
		[HttpGet("Admin-endpoint")]
		[Authorize(Roles = "Admin")]
		public ActionResult AdminCheck()
		{
			return Ok();
		}
	}
}
