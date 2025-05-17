namespace Auth_with_JWT.Models
{
	public class RefreshTokenRequestDTO
	{
		public Guid UserId { get; set; }
		public string RefreshToken { get; set; } = string.Empty;

	}
}
