using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Features.Dict.Region
{
	public class EfRegionRepo : IRegionDictRepo
	{
		private readonly AppDbContext _db;

		public EfRegionRepo(AppDbContext db) { _db = db; }

		public void Insert(List<RegionFullDto> items)
		{
			var entities = items.Select(i => new RegionEntity { Id = i.Id, Name = i.Name, Enabled = i.Enabled }).ToList();
			_db.Set<RegionEntity>().AddRange(entities);
			_db.SaveChanges();
		}

		public List<RegionDto> Fetch()
		{
			return _db.Set<RegionEntity>().AsNoTracking().Where(x => x.Enabled).OrderBy(x => x.Id)
				.Select(x => new RegionDto { Id = x.Id, Name = x.Name }).ToList();
		}

		public List<RegionFullDto> FetchFull()
		{
			return _db.Set<RegionEntity>().AsNoTracking().OrderBy(x => x.Id)
				.Select(x => new RegionFullDto { Id = x.Id, Name = x.Name, Enabled = x.Enabled }).ToList();
		}

		public void Update(List<RegionFullDto> items)
		{
			foreach (var it in items)
			{
				var e = _db.Set<RegionEntity>().Find(it.Id);
				if (e == null) continue;
				e.Name = it.Name;
				e.Enabled = it.Enabled;
			}
			_db.SaveChanges();
		}

		public void Delete(int id)
		{
			var e = _db.Set<RegionEntity>().Find(id);
			if (e == null) return;
			_db.Set<RegionEntity>().Remove(e);
			_db.SaveChanges();
		}
	}
}
