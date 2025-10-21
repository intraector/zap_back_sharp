using System;
using System.Linq;
using App.Data;
using App.Data.Entities;
using Microsoft.Extensions.Configuration;

namespace App.Features.Auth.Services
{
	public class TokenService : ITokenService
	{
		private readonly AppDbContext _db;
		private readonly IConfiguration _config;

		public TokenService(AppDbContext db, IConfiguration config)
		{
			_db = db;
			_config = config;
		}

		public string CreateAccessToken(string userId, int[]? roles = null)
		{
			var jwtCfg = _config.GetSection("Jwt");
			var key = jwtCfg.GetValue<string>("Key") ?? throw new Exception("Jwt key not configured");
			var issuer = jwtCfg.GetValue<string>("Issuer") ?? "zap";
			var audience = jwtCfg.GetValue<string>("Audience") ?? "zap";
			var minutes = jwtCfg.GetValue<int>("AccessMinutes");
			var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
			var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes);
			// Ensure the key has the same KeyId as configured in the server so the token header includes a matching 'kid'.
			using (var sha = System.Security.Cryptography.SHA256.Create())
			{
				var hash = sha.ComputeHash(keyBytes);
				var kid = Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
				securityKey.KeyId = kid;
			}
			var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
			var now = DateTime.UtcNow;
			var claimsList = new System.Collections.Generic.List<System.Security.Claims.Claim>();
			claimsList.Add(new System.Security.Claims.Claim("user_id", userId));

			string[]? roleNames = null;
			if (roles != null)
			{
				var map = new System.Collections.Generic.Dictionary<int, string>
				{
					[0] = "superuser",
					[1] = "admin",
					[2] = "user",
					[3] = "partner"
				};
				roleNames = roles.Select(r => map.ContainsKey(r) ? map[r] : r.ToString()).ToArray();
			}

			var payload = new System.IdentityModel.Tokens.Jwt.JwtPayload(
				issuer: issuer,
				audience: audience,
				claims: claimsList,
				notBefore: now,
				expires: now.AddMinutes(minutes)
			);

			if (roleNames != null)
			{
				payload["roles"] = roleNames;
			}

			payload["sub"] = userId;
			payload["jti"] = System.Guid.NewGuid().ToString();

			var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
				new System.IdentityModel.Tokens.Jwt.JwtHeader(creds),
				payload
			);

			return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
		}

		public string CreateRefreshToken(string userId)
		{
			var exp = DateTimeOffset.UtcNow.AddDays(14).ToUnixTimeSeconds();
			var raw = new byte[32];
			using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
			{
				rng.GetBytes(raw);
			}
			string randomPart = Convert.ToBase64String(raw).TrimEnd('=');
			var token = $"refresh:{userId}:{exp}:{randomPart}";

			var hash = HashToken(token);

			if (int.TryParse(userId, out var uid))
			{
				var entity = new RefreshToken { UserId = uid, Token = hash, CreatedAt = DateTime.UtcNow };
				_db.RefreshTokens.Add(entity);
				_db.SaveChanges();
			}

			return token;
		}

		public bool ValidateToken(string token)
		{
			if (string.IsNullOrEmpty(token)) return false;
			var parts = token.Split(':');
			if (token.Count(c => c == '.') == 2)
			{
				var jwtCfg = _config.GetSection("Jwt");
				var key = jwtCfg.GetValue<string>("Key") ?? string.Empty;
				var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtCfg.GetValue<string>("Issuer"),
					ValidAudience = jwtCfg.GetValue<string>("Audience"),
					IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key))
				};
				try
				{
					var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
					handler.ValidateToken(token, validationParameters, out var validated);
					return true;
				}
				catch { return false; }
			}

			if (parts.Length != 4) return false;
			if (!long.TryParse(parts[2], out var exp)) return false;
			if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= exp) return false;

			if (parts[0] == "refresh")
			{
				var presentedHash = HashToken(token);
				var entity = _db.RefreshTokens.FirstOrDefault(r => r.Token == presentedHash);
				if (entity == null) return false;
				if ((DateTime.UtcNow - entity.CreatedAt).TotalDays > 30) return false;
				return true;
			}

			return false;
		}

		public string RotateRefreshToken(string presentedToken, string userId)
		{
			if (string.IsNullOrEmpty(presentedToken)) return string.Empty;
			var presentedHash = HashToken(presentedToken);
			var entity = _db.RefreshTokens.FirstOrDefault(r => r.Token == presentedHash);
			if (entity != null)
			{
				_db.RefreshTokens.Remove(entity);
				_db.SaveChanges();
			}
			return CreateRefreshToken(userId);
		}

		public string? ExtractUserId(string token)
		{
			if (string.IsNullOrEmpty(token)) return null;
			var parts = token.Split(':');
			if (parts.Length >= 2) return parts[1];
			return null;
		}

		private static string HashToken(string token)
		{
			using var sha = System.Security.Cryptography.SHA256.Create();
			var bytes = System.Text.Encoding.UTF8.GetBytes(token);
			var hash = sha.ComputeHash(bytes);
			return Convert.ToBase64String(hash).TrimEnd('=');
		}
	}
}
