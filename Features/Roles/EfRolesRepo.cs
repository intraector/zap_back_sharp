using System.Collections.Generic;
using System.Linq;
using App.Data;
using App.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Features.Roles
{
	public class EfRolesRepo : IRolesRepo
	{
		private readonly AppDbContext _db;
		public EfRolesRepo(AppDbContext db) { _db = db; }

		public List<RoleDto> Fetch()
		{
			return _db.Set<RolePathEntity>().AsNoTracking().OrderBy(x => x.Id)
				.Select(x => new RoleDto { Id = x.Role, Name = x.Label }).ToList();
		}

		public void Insert(List<RoleDto> items)
		{
			var entities = items.Select(i => new RolePathEntity { Path = "", Role = i.Id, Label = i.Name }).ToList();
			_db.Set<RolePathEntity>().AddRange(entities);
			_db.SaveChanges();
		}
	}
}
