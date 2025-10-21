using System;
using System.Collections.Generic;

namespace App.Features.Accounts
{
	public class AccountDto { public int Id { get; set; } public string? Phone { get; set; } public int Status { get; set; } public int[] Roles { get; set; } = Array.Empty<int>(); public DateTime CreatedAt { get; set; } }
}
