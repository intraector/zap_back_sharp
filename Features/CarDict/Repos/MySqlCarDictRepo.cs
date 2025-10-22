using System.Collections.Generic;
using App.Data.Entities.CarDict;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace App.Features.CarDict.Repos
{
	public class MySqlCarDictRepo : ICarDictRepo
	{
		private readonly string _conn;
		private readonly ILogger<MySqlCarDictRepo> _logger;
		public MySqlCarDictRepo(IConfiguration cfg, ILogger<MySqlCarDictRepo> logger)
		{
			_conn = cfg.GetConnectionString("MySql") ?? string.Empty;
			_logger = logger;
		}

		public List<BrandEntity> FetchBrands(string query, int pageNumber, int pageSize)
		{
			var list = new List<BrandEntity>();
			try
			{
				using var conn = new MySqlConnection(_conn);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT id_car_mark, name FROM car_mark WHERE LOWER(name) LIKE @q LIMIT @limit OFFSET @off";
				cmd.Parameters.AddWithValue("@q", $"%{query.ToLower()}%");
				cmd.Parameters.AddWithValue("@limit", pageSize);
				cmd.Parameters.AddWithValue("@off", (pageNumber - 1) * pageSize);
				using var r = cmd.ExecuteReader();
				while (r.Read())
				{
					list.Add(new BrandEntity { Id = r.GetInt32(0), Name = r.GetString(1) });
				}
			}
			catch (MySqlConnector.MySqlException ex)
			{
				_logger.LogError(ex, "MySql error in FetchBrands (query={Query}, page={Page}, size={Size})", query, pageNumber, pageSize);
				throw;
			}
			return list;
		}

		public List<ModelEntity> FetchModels(int brandId, string query, int pageNumber, int pageSize)
		{
			var list = new List<ModelEntity>();
			try
			{
				using var conn = new MySqlConnection(_conn);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT id_car_model, id_car_mark, name FROM car_model WHERE id_car_mark = @brandId AND LOWER(name) LIKE @q LIMIT @limit OFFSET @off";
				cmd.Parameters.AddWithValue("@brandId", brandId);
				cmd.Parameters.AddWithValue("@q", $"%{query.ToLower()}%");
				cmd.Parameters.AddWithValue("@limit", pageSize);
				cmd.Parameters.AddWithValue("@off", (pageNumber - 1) * pageSize);
				using var r = cmd.ExecuteReader();
				while (r.Read())
				{
					list.Add(new ModelEntity { Id = r.GetInt32(0), BrandId = r.GetInt32(1), Name = r.GetString(2) });
				}
			}
			catch (MySqlConnector.MySqlException ex)
			{
				_logger.LogError(ex, "MySql error in FetchModels (brandId={BrandId}, query={Query}, page={Page}, size={Size})", brandId, query, pageNumber, pageSize);
				throw;
			}
			return list;
		}

		public List<BodyEntity> FetchBodies(int modelId, int? generationId, string query, int pageNumber, int pageSize)
		{
			var list = new List<BodyEntity>();
			try
			{
				using var conn = new MySqlConnection(_conn);
				conn.Open();
				var cmd = conn.CreateCommand();
				var sql = "SELECT id_car_serie, id_car_model, id_car_generation, name FROM car_serie WHERE id_car_model = @modelId";
				if (generationId.HasValue)
				{
					sql += " AND id_car_generation = @generationId";
					cmd.Parameters.AddWithValue("@generationId", generationId.Value);
				}
				sql += " AND LOWER(name) LIKE @q LIMIT @limit OFFSET @off";
				cmd.CommandText = sql;
				cmd.Parameters.AddWithValue("@modelId", modelId);
				cmd.Parameters.AddWithValue("@q", $"%{query.ToLower()}%");
				cmd.Parameters.AddWithValue("@limit", pageSize);
				cmd.Parameters.AddWithValue("@off", (pageNumber - 1) * pageSize);
				using var r = cmd.ExecuteReader();
				while (r.Read())
				{
					list.Add(new BodyEntity { Id = r.GetInt32(0), ModelId = r.GetInt32(1), GenerationId = r.IsDBNull(2) ? (int?)null : r.GetInt32(2), Name = r.GetString(3) });
				}
			}
			catch (MySqlConnector.MySqlException ex)
			{
				_logger.LogError(ex, "MySql error in FetchBodies (modelId={ModelId}, generationId={GenerationId}, query={Query}, page={Page}, size={Size})", modelId, generationId, query, pageNumber, pageSize);
				throw;
			}
			return list;
		}

		public List<GenerationEntity> FetchGenerations(int modelId, string? query, int? pageNumber, int? pageSize)
		{
			var list = new List<GenerationEntity>();
			try
			{
				using var conn = new MySqlConnection(_conn);
				conn.Open();
				var cmd = conn.CreateCommand();
				var sql = "SELECT id_car_generation, id_car_model, name, year_begin, year_end FROM car_generation WHERE id_car_model = @modelId AND LOWER(name) LIKE @q LIMIT @limit OFFSET @off";
				cmd.Parameters.AddWithValue("@modelId", modelId);
				cmd.CommandText = sql;
				var qVal = (query ?? string.Empty).ToLower();
				var pn = pageNumber.GetValueOrDefault(1);
				var ps = pageSize.GetValueOrDefault(20);
				cmd.Parameters.AddWithValue("@q", $"%{qVal}%");
				cmd.Parameters.AddWithValue("@limit", ps);
				cmd.Parameters.AddWithValue("@off", (pn - 1) * ps);

				using var r = cmd.ExecuteReader();
				while (r.Read())
				{
					list.Add(new GenerationEntity
					{
						Id = r.GetInt32(0),
						ModelId = r.GetInt32(1),
						Name = r.GetString(2),
						YearFrom = r.IsDBNull(3) ? null : r.GetString(3),
						YearTo = r.IsDBNull(4) ? null : r.GetString(4),
					});
				}
			}
			catch (MySqlConnector.MySqlException ex)
			{
				// pn and ps are defined above; use their values for logging
				var logPn = pageNumber.GetValueOrDefault(1);
				var logPs = pageSize.GetValueOrDefault(20);
				_logger.LogError(ex, "MySql error in FetchGenerations (modelId={ModelId}, query={Query}, page={Page}, size={Size})", modelId, query, logPn, logPs);
				throw;
			}
			return list;
		}

		public List<ModificationEntity> FetchModifications(int modelId, int bodyId, string query, int pageNumber, int pageSize)
		{
			var list = new List<ModificationEntity>();
			try
			{
				using var conn = new MySqlConnection(_conn);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT id_car_modification, id_car_model, id_car_serie, name FROM car_modification WHERE id_car_model = @modelId AND id_car_serie = @bodyId AND LOWER(name) LIKE @q LIMIT @limit OFFSET @off";
				cmd.Parameters.AddWithValue("@modelId", modelId);
				cmd.Parameters.AddWithValue("@bodyId", bodyId);
				cmd.Parameters.AddWithValue("@q", $"%{query.ToLower()}%");
				cmd.Parameters.AddWithValue("@limit", pageSize);
				cmd.Parameters.AddWithValue("@off", (pageNumber - 1) * pageSize);
				using var r = cmd.ExecuteReader();
				while (r.Read())
				{
					list.Add(new ModificationEntity { Id = r.GetInt32(0), ModelId = r.GetInt32(1), BodyId = r.GetInt32(2), Name = r.GetString(3) });
				}
			}
			catch (MySqlConnector.MySqlException ex)
			{
				_logger.LogError(ex, "MySql error in FetchModifications (modelId={ModelId}, bodyId={BodyId}, query={Query}, page={Page}, size={Size})", modelId, bodyId, query, pageNumber, pageSize);
				throw;
			}
			return list;
		}
	}
}
