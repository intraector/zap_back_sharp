namespace App.Data.Entities.CarDict
{
	public class GenerationEntity
	{
		public int Id { get; set; }
		public int ModelId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? YearFrom { get; set; }
		public string? YearTo { get; set; }
	}
}
