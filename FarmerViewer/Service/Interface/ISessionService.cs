using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface ISessionService
    {
        //public Task StoreSessionInRedis(string sessionId, string userToken);
        //public Task<string> GetSessionFromRedis(string sessionId);
        //public Task<bool> RemoveKey(string sessionId);
        public Task StoreSessionInMemoryCache(string sessionId, string userToken);
        public Task<bool> GetSessionFromMemoryCache(string userId,string seassionId);
        public Task<bool> RemoveKeyMemoryCache(string sessionId);


    }
}
