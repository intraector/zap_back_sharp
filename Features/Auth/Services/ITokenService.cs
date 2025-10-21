namespace App.Features.Auth.Services
{
	public interface ITokenService
	{
		string CreateAccessToken(string userId, int[]? roles = null);
		string CreateRefreshToken(string userId);
		string RotateRefreshToken(string presentedToken, string userId);
		bool ValidateToken(string token);
		string? ExtractUserId(string token);
	}
}
