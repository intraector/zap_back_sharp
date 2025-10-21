using System.Collections.Generic;

namespace App.Features.Dict.Region
{
	public interface IRegionDictRepo
	{
		void Insert(List<RegionFullDto> items);
		List<RegionDto> Fetch();
		List<RegionFullDto> FetchFull();
		void Update(List<RegionFullDto> items);
		void Delete(int id);
	}
}
