using BusinessEntity;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
namespace BusinessFacade
{
    public interface IBroadcast : Orleans.IGrainWithIntegerKey
    {
        /// <summary>
        /// 检测函数
        /// </summary>
        /// <param name="login"></param>
        /// <returns>WorkFlag 2:正常/1:有消息需要刷新/0：Failure;</returns>
        Task<BroadcastMessage> Check(UserLoginEntity login);


        Task<BroadcastEntity> Create(BroadcastEntity model);
        Task<BroadcastEntity> Updates(BroadcastEntity model);
        Task<BroadcastEntity> Confirm(BroadcastEntity model);
        Task<BroadcastEntity> GetModel(int AutoId);
        Task AddMessage(BroadcastMessage mess);
        


    }
}
