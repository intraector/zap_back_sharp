using App.Data.Entities.CarDict;
using App.Features.CarDict.Dtos;
using App.Features.CarDict.Repos;
using Microsoft.AspNetCore.Mvc;

namespace App.Features.CarDict
{
	[ApiController]
	[Route("api/V1/dict/cars")]
	public class CarDictController : ControllerBase
	{
		private readonly ICarDictRepo _repo;

		public CarDictController(ICarDictRepo repo)
		{
			_repo = repo;
		}

		[HttpGet("brands")]
		[Microsoft.AspNetCore.Authorization.AllowAnonymous]
		public IActionResult GetBrands([FromQuery] string? q = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
		{
			var items = _repo.FetchBrands(q ?? string.Empty, pageNumber, pageSize);
			var resp = new GenerationsResp<BrandEntity> { PageNumber = pageNumber, PageSize = pageSize, NoMorePages = items.Count < pageSize, Data = items };
			return Ok(resp);
		}

		[HttpGet("models")]
		public IActionResult GetModels([FromQuery] int? brandId = null, [FromQuery] string? q = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
		{
			if (brandId == null) return BadRequest("brandId");
			var items = _repo.FetchModels(brandId.Value, q ?? string.Empty, pageNumber, pageSize);
			var resp = new GenerationsResp<ModelEntity> { PageNumber = pageNumber, PageSize = pageSize, NoMorePages = items.Count < pageSize, Data = items };
			return Ok(resp);
		}

		[HttpGet("generations")]
		public IActionResult GetGenerations([FromQuery] int? modelId = null, [FromQuery] string? q = null, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
		{
			if (modelId == null) return BadRequest("modelId");
			var items = _repo.FetchGenerations(modelId.Value, q, pageNumber, pageSize);
			var pn = pageNumber.GetValueOrDefault(1);
			var ps = pageSize.GetValueOrDefault(20);
			var resp = new GenerationsResp<GenerationEntity> { PageNumber = pn, PageSize = ps, NoMorePages = items.Count < ps, Data = items };
			return Ok(resp);
		}

		[HttpGet("bodies")]
		public IActionResult GetBodies([FromQuery] int? modelId = null, [FromQuery] int? generationId = null, [FromQuery] string? q = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
		{
			if (modelId == null) return BadRequest("modelId");
			var items = _repo.FetchBodies(modelId.Value, generationId, q ?? string.Empty, pageNumber, pageSize);
			var resp = new GenerationsResp<BodyEntity> { PageNumber = pageNumber, PageSize = pageSize, NoMorePages = items.Count < pageSize, Data = items };
			return Ok(resp);
		}

		[HttpGet("modifications")]
		public IActionResult GetModifications([FromQuery] int? modelId = null, [FromQuery] int? bodyId = null, [FromQuery] string? q = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
		{
			if (modelId == null) return BadRequest("modelId");
			if (bodyId == null) return BadRequest("bodyId");
			var items = _repo.FetchModifications(modelId.Value, bodyId.Value, q ?? string.Empty, pageNumber, pageSize);
			var resp = new GenerationsResp<ModificationEntity> { PageNumber = pageNumber, PageSize = pageSize, NoMorePages = items.Count < pageSize, Data = items };
			return Ok(resp);
		}
	}
}
