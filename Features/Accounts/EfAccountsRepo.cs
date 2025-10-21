using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Features.Accounts
{
	public class EfAccountsRepo : IAccountsRepo
	{
		private readonly AppDbContext _db;

		public EfAccountsRepo(AppDbContext db) { _db = db; }

		public List<AccountDto> Fetch()
		{
			return _db.Accounts.AsNoTracking().OrderBy(x => x.Id).Select(a => new AccountDto
			{
				Id = a.Id,
				Phone = a.Phone,
				Status = a.Status,
				Roles = a.Roles ?? System.Array.Empty<int>(),
				CreatedAt = a.CreatedAt
			}).ToList();
		}

		public async Task<AccountDto?> GetByIdAsync(int id)
		{
			var a = await _db.Accounts.FindAsync(id);
			if (a == null) return null;
			return new AccountDto { Id = a.Id, Phone = a.Phone, Status = a.Status, Roles = a.Roles ?? System.Array.Empty<int>(), CreatedAt = a.CreatedAt };
		}

		public async Task<AccountDto?> GetByPhoneAsync(string phone)
		{
			var a = await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Phone == phone);
			if (a == null) return null;
			return new AccountDto { Id = a.Id, Phone = a.Phone, Status = a.Status, Roles = a.Roles ?? System.Array.Empty<int>(), CreatedAt = a.CreatedAt };
		}

		public async Task<AccountDto> CreateAsync(AccountDto account)
		{
			var e = new Account
			{
				Phone = account.Phone,
				Status = account.Status,
				Roles = account.Roles ?? System.Array.Empty<int>(),
				CreatedAt = account.CreatedAt == default ? System.DateTime.UtcNow : account.CreatedAt
			};
			_db.Accounts.Add(e);
			await _db.SaveChangesAsync();
			account.Id = e.Id;
			account.CreatedAt = e.CreatedAt;
			return account;
		}
	}
}
