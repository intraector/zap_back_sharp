using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Features.Accounts
{
	public interface IAccountsRepo
	{
		List<AccountDto> Fetch();
		Task<AccountDto?> GetByIdAsync(int id);
		Task<AccountDto?> GetByPhoneAsync(string phone);
		Task<AccountDto> CreateAsync(AccountDto account);
	}
}
