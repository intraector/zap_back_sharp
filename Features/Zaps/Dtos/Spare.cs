using System.Collections.Generic;

namespace App.Features.Zaps.Dtos
{
	public class Spare
	{
		public int Id { get; set; }
		public int ZapId { get; set; }
		public string Description { get; set; } = string.Empty;
		public string Status { get; set; } = "0";
		public List<string> Photos { get; set; } = new();
	}
}
