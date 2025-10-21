using System;
using System.Collections.Generic;

namespace App.Data.Entities
{
	public class Account
	{
		public int Id { get; set; }
		public string? Phone { get; set; }
		public int Status { get; set; }
		public int[] Roles { get; set; } = System.Array.Empty<int>();
		public DateTime CreatedAt { get; set; }
	}
}
