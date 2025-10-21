using System.Collections.Generic;
using App.Data.Entities.CarDict;

namespace App.Features.CarDict.Repos
{
	public interface ICarDictRepo
	{
		List<BrandEntity> FetchBrands(string query, int pageNumber, int pageSize);
		List<ModelEntity> FetchModels(int brandId, string query, int pageNumber, int pageSize);
		List<BodyEntity> FetchBodies(int modelId, int? generationId, string query, int pageNumber, int pageSize);
		List<GenerationEntity> FetchGenerations(int modelId, string? query, int? pageNumber, int? pageSize);
		List<ModificationEntity> FetchModifications(int modelId, int bodyId, string query, int pageNumber, int pageSize);
	}
}
