namespace App.Data.Entities.CarDict
{
	public class BodyEntity
	{
		public int Id { get; set; }
		public int ModelId { get; set; }
		public int? GenerationId { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}
