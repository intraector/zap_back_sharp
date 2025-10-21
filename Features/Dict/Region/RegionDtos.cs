using System.Collections.Generic;

namespace App.Features.Dict.Region
{
	public class RegionDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
	public class RegionFullDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; public bool Enabled { get; set; } }
}
