using System.Collections.Generic;

namespace App.Features.Zaps.Dtos
{
	public class ZapsResp
	{
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public bool NoMorePages { get; set; }
		public List<Zap> Data { get; set; } = new();
	}
}
