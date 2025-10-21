using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace App.Features.Dict.Region
{
	[ApiController]
	[Route("api/V1/dict/region")]
	public class RegionController : ControllerBase
	{
		private readonly IRegionDictRepo _repo;

		public RegionController(IRegionDictRepo repo)
		{
			_repo = repo;
		}

		[HttpGet]
		[Microsoft.AspNetCore.Authorization.AllowAnonymous]
		public ActionResult<IEnumerable<RegionDto>> Get() => Ok(_repo.Fetch());

		[HttpPost]
		public ActionResult Insert([FromBody] List<RegionFullDto> items)
		{
			_repo.Insert(items);
			return Ok();
		}

		[HttpPut("/api/V1/admin/dict/region/")]
		public ActionResult Update([FromBody] List<RegionFullDto> items)
		{
			_repo.Update(items);
			return Ok();
		}

		[HttpDelete("/api/V1/admin/dict/region/{id}")]
		public ActionResult Delete(int id)
		{
			_repo.Delete(id);
			return Ok();
		}
	}
}
