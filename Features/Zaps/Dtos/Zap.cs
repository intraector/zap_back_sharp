using System.Collections.Generic;

namespace App.Features.Zaps.Dtos
{
	public class Zap
	{
		public int Id { get; set; }
		public int BrandId { get; set; }
		public int ModelId { get; set; }
		public int BodyId { get; set; }
		public int? GenerationId { get; set; }
		public int ModificationId { get; set; }
		public Region.Default Region { get; set; } = new Region.Default();
		public string Description { get; set; } = string.Empty;
		public List<Spare> Spares { get; set; } = new();
	}



}
