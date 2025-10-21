using System;

namespace App.Data.Entities
{
	public class PhoneCode
	{
		public int Id { get; set; }
		public string Phone { get; set; } = string.Empty;
		public string Code { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
	}
}
