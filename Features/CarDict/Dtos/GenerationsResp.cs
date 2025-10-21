namespace App.Features.CarDict.Dtos
{
	public class GenerationsResp<T>
	{
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public bool NoMorePages { get; set; }
		public System.Collections.Generic.List<T> Data { get; set; } = new System.Collections.Generic.List<T>();
	}
}
