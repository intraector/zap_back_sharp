using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace App.Features.Roles
{
	[ApiController]
	[Route("api/v1/admin/roles")]
	public class RolesController : ControllerBase
	{
		private readonly IRolesRepo _repo;
		public RolesController(IRolesRepo repo) { _repo = repo; }

		[HttpGet]
		public ActionResult<IEnumerable<RoleDto>> Get() => Ok(_repo.Fetch());

		[HttpPost]
		public ActionResult Insert([FromBody] List<RoleDto> items) { _repo.Insert(items); return Ok(); }
	}
}
