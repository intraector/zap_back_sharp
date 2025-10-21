using System.Collections.Generic;

namespace App.Data.Entities
{
	public class SpareEntity
	{
		public int Id { get; set; }
		public int ZapId { get; set; }
		public string Description { get; set; } = string.Empty;
		public int Status { get; set; }
		public string Photos { get; set; } = string.Empty; // store as JSON array string

		public ZapEntity? Zap { get; set; }
	}
}
