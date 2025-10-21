using System;

namespace App.Data.Entities
{
	public class RegionEntity
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public bool Enabled { get; set; }
	}
}
