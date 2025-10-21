using System.Threading.Tasks;
using App.Data;
using App.Features.Zaps.Dtos;
using App.Features.Zaps.Repos;
using Microsoft.AspNetCore.Mvc;

namespace App.Features.Zaps
{
	[ApiController]
	[Route("api/v1/zaps")]
	public class ZapsController : ControllerBase
	{
		private readonly IZapsRepo _repo;
		private readonly AppDbContext _db;

		public ZapsController(IZapsRepo repo, AppDbContext db)
		{
			_repo = repo;
			_db = db;
		}

		[HttpGet]
		public IActionResult Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
		{
			var req = new ZapsReq { PageNumber = page, PageSize = pageSize };
			var items = _repo.Fetch(req);
			return Ok(items);
		}

		[HttpPost]
		public IActionResult Post([FromForm] Zap req)
		{
			var id = _repo.Create(req);
			return Ok(new { id = id });
		}

		[HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			_repo.Delete(id);
			return Ok();
		}
	}
}
