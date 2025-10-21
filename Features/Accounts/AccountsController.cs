using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace App.Features.Accounts
{
	[ApiController]
	[Route("api/accounts")]
	public class AccountsController : ControllerBase
	{
		private readonly IAccountsRepo _repo;

		public AccountsController(IAccountsRepo repo) { _repo = repo; }

		[HttpGet("by-phone")]
		public async Task<IActionResult> GetByPhone([FromQuery] string phone)
		{
			var a = await _repo.GetByPhoneAsync(phone);
			if (a == null) return NotFound();
			return Ok(a);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] AccountDto dto)
		{
			var created = await _repo.CreateAsync(dto);
			return CreatedAtAction(nameof(GetByPhone), new { phone = created.Phone }, created);
		}
	}
}
