using System.Linq;
using System.Text.Json;
using App.Data;
using App.Data.Entities;
using App.Features.Zaps.Dtos;
using Microsoft.EntityFrameworkCore;

namespace App.Features.Zaps.Repos
{
	public class EfZapsRepo : IZapsRepo
	{
		private readonly AppDbContext _db;
		public EfZapsRepo(AppDbContext db) { _db = db; }

		public ZapsResp Fetch(ZapsReq req)
		{
			var query = _db.Zaps.Include(z => z.Spares).AsQueryable();
			var page = req.PageNumber < 1 ? 1 : req.PageNumber;
			var data = query.Skip((page - 1) * req.PageSize).Take(req.PageSize).ToList();
			var zaps = data.Select(z => new Zap
			{
				Id = z.Id,
				BrandId = z.BrandId,
				ModelId = z.ModelId,
				BodyId = z.BodyId,
				GenerationId = z.GenerationId,
				ModificationId = z.ModificationId,
				Region = new Region.Default { Id = z.RegionId, Name = z.RegionName },
				Description = z.Description,
				Spares = z.Spares.Select(s => new Spare { Id = s.Id, ZapId = s.ZapId, Description = s.Description, Status = s.Status.ToString(), Photos = JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(s.Photos) ?? new() }).ToList()
			}).ToList();

			return new ZapsResp { PageNumber = req.PageNumber, PageSize = req.PageSize, NoMorePages = zaps.Count < req.PageSize, Data = zaps };
		}

		public Zap Create(Zap zap)
		{
			var entity = new ZapEntity
			{
				BrandId = zap.BrandId,
				ModelId = zap.ModelId,
				BodyId = zap.BodyId,
				GenerationId = zap.GenerationId,
				ModificationId = zap.ModificationId,
				RegionId = zap.Region.Id,
				RegionName = zap.Region.Name,
				Description = zap.Description
			};
			_db.Zaps.Add(entity);
			_db.SaveChanges();

			// Add spares
			for (int i = 0; i < zap.Spares.Count; i++)
			{
				var s = zap.Spares[i];
				var se = new SpareEntity { ZapId = entity.Id, Description = s.Description, Status = int.TryParse(s.Status, out var st) ? st : 0, Photos = JsonSerializer.Serialize(s.Photos) };
				_db.Spares.Add(se);
			}
			_db.SaveChanges();

			// Reload with ids
			var created = _db.Zaps.Include(z => z.Spares).First(z => z.Id == entity.Id);
			return new Zap
			{
				Id = created.Id,
				BrandId = created.BrandId,
				ModelId = created.ModelId,
				BodyId = created.BodyId,
				GenerationId = created.GenerationId,
				ModificationId = created.ModificationId,
				Region = new Region.Default { Id = created.RegionId, Name = created.RegionName },
				Description = created.Description,
				Spares = created.Spares.Select(s => new Spare { Id = s.Id, ZapId = s.ZapId, Description = s.Description, Status = s.Status.ToString(), Photos = JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(s.Photos) ?? new() }).ToList()
			};
		}

		public void Delete(int id)
		{
			var spares = _db.Spares.Where(s => s.ZapId == id);
			_db.Spares.RemoveRange(spares);
			var zap = _db.Zaps.Find(id);
			if (zap != null) _db.Zaps.Remove(zap);
			_db.SaveChanges();
		}
	}
}
