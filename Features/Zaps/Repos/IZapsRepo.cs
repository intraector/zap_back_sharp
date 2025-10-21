using App.Features.Zaps.Dtos;

namespace App.Features.Zaps.Repos
{
	public interface IZapsRepo
	{
		ZapsResp Fetch(ZapsReq req);
		Zap Create(Zap zap);
		void Delete(int id);
	}
}
