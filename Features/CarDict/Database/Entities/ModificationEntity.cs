namespace App.Data.Entities.CarDict
{
	public class ModificationEntity
	{
		public int Id { get; set; }
		public int ModelId { get; set; }
		public int BodyId { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}
