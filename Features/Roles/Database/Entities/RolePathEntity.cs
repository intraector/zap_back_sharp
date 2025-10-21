namespace App.Data.Entities
{
	public class RolePathEntity
	{
		public int Id { get; set; }
		public string Path { get; set; } = string.Empty;
		public int Role { get; set; }
		public string Label { get; set; } = string.Empty;
	}
}
