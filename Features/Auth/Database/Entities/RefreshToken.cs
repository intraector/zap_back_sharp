using System;

namespace App.Data.Entities
{
	public class RefreshToken
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Token { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
	}
}
