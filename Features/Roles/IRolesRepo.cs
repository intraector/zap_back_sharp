using System.Collections.Generic;

namespace App.Features.Roles
{
	public interface IRolesRepo
	{
		List<RoleDto> Fetch();
		void Insert(List<RoleDto> items);
	}
}
