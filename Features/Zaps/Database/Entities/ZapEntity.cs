using System.Collections.Generic;

namespace App.Data.Entities
{
	public class ZapEntity
	{
		public int Id { get; set; }
		public int BrandId { get; set; }
		public int ModelId { get; set; }
		public int BodyId { get; set; }
		public int? GenerationId { get; set; }
		public int ModificationId { get; set; }
		public int RegionId { get; set; }
		public string RegionName { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public List<SpareEntity> Spares { get; set; } = new();
	}
}
