using System.Threading.Tasks;
using App.Data;
using App.Data.Entities;
using App.Features.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Features.Auth
{
	[ApiController]
	[Route("api/v1/auth")]
	public class AuthController : ControllerBase
	{
		private readonly ITokenService _tokens;
		private readonly AppDbContext _db;

		public AuthController(ITokenService tokens, AppDbContext db)
		{
			_tokens = tokens;
			_db = db;
		}

		[HttpGet("phone-code")]
		[Microsoft.AspNetCore.Authorization.AllowAnonymous]
		public async Task<IActionResult> GetPhoneCode([FromQuery] string phone)
		{
			if (string.IsNullOrWhiteSpace(phone)) return BadRequest("phone");
			var code = "000000";
			var entity = new PhoneCode { Phone = phone, Code = code, CreatedAt = System.DateTime.UtcNow };
			_db.PhoneCodes.Add(entity);
			await _db.SaveChangesAsync();
			return Ok();
		}



		public class PhoneCodeReq { public string Phone { get; set; } = string.Empty; public string Code { get; set; } = string.Empty; }
		public class TokensResp { public string Access { get; set; } = string.Empty; public string Refresh { get; set; } = string.Empty; }

		[HttpPost("sign-in-with-phone")]
		[Microsoft.AspNetCore.Authorization.AllowAnonymous]
		public async Task<IActionResult> SignInWithPhone([FromBody] PhoneCodeReq req)
		{
			if (req.Code?.Length != 6) return BadRequest("wrong code");
			if (string.IsNullOrWhiteSpace(req.Phone)) return BadRequest("phone");
			var stored = await _db.PhoneCodes
				.Where(pc => pc.Phone == req.Phone && pc.Code == req.Code)
				.OrderByDescending(pc => pc.CreatedAt)
				.FirstOrDefaultAsync();
			if (stored == null)
			{
				return StatusCode(StatusCodes.Status403Forbidden);
			}

			_db.PhoneCodes.Remove(stored);
			await _db.SaveChangesAsync();

			var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Phone == req.Phone);
			if (account == null)
			{
				account = new App.Data.Entities.Account { Phone = req.Phone, CreatedAt = System.DateTime.UtcNow, Status = 1 };
				_db.Accounts.Add(account);
				await _db.SaveChangesAsync();
			}

			var access = _tokens.CreateAccessToken(account.Id.ToString(), account.Roles);
			var refresh = _tokens.CreateRefreshToken(account.Id.ToString());
			return Ok(new TokensResp { Access = access, Refresh = refresh });
		}

		public class RefreshReq { public string Token { get; set; } = string.Empty; }

		[HttpPost("refresh-token")]
		[Microsoft.AspNetCore.Authorization.AllowAnonymous]
		public async Task<IActionResult> Refresh([FromBody] RefreshReq req)
		{
			if (string.IsNullOrWhiteSpace(req.Token)) return Unauthorized();
			if (!_tokens.ValidateToken(req.Token)) return Unauthorized();
			var user = _tokens.ExtractUserId(req.Token) ?? "0";
			var roles = await _db.Accounts.Where(a => a.Id.ToString() == user).Select(a => a.Roles).FirstOrDefaultAsync();
			var access = _tokens.CreateAccessToken(user, roles);
			var refresh = _tokens.RotateRefreshToken(req.Token, user);
			await _db.SaveChangesAsync();
			return Ok(new TokensResp { Access = access, Refresh = refresh });
		}
	}
}
