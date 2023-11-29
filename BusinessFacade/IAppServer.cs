using BusinessEntity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessFacade
{
    public interface IAppServer : Orleans.IGrainWithIntegerKey
    {
        Task<int> RefreshCache(string key);
        Task<int> AddLogin(SimpleUserLoginEntity user);
        Task<bool> RemoveLogin(Guid LoginCode);

        Task<string> CheckUpdateVersion(string curversion, string guidid);
        Task<ClientFtpUpdateEntity> GetNeedUpdateFiles(string curversion, string localcode);
        Task<Dictionary<string, SimpleUserLoginEntity>> GetLoginedUsers();
        Task<bool> ClearVersionCaches();
    }
}
