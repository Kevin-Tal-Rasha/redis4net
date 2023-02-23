using System;
using System.Threading.Tasks;

namespace redis4net.Redis
{
    using redis4net.Util.Encryption;
    using StackExchange.Redis;
    using System.Text;

    public class Connection : IConnection, IDisposable
    {
        private string _listName;
        private string _listNameSuffix;
        private ConnectionMultiplexer redis;

        public void Open(string connString, string listName, string listNameSuffix)
        {
            try
            {
                _listName = listName;
                _listNameSuffix = listNameSuffix;
                redis = ConnectionMultiplexer.Connect(connString);
            }
            catch
            {
                redis = null;
            }
        }

        public void Open(string hostname, int port, string listName, string password
                       , string listNameSuffix, int database)
        {
            try
            {
                _listName = listName;
                _listNameSuffix = listNameSuffix;
                StringBuilder connStr = new StringBuilder($"{hostname}:{port},defaultDatabase={database}");
                if (!string.IsNullOrEmpty(password))
                    connStr.Append($",password={password.Decrypt()}");
                redis = ConnectionMultiplexer.Connect(connStr.ToString());
            }
            catch
            {
                redis = null;
            }
        }

        public bool IsOpen()
        {
            return redis != null;
        }

        public Task<long> AddToList(string content)
        {
            var database = redis.GetDatabase();
            string key = _listName;
            if (!string.IsNullOrEmpty(_listNameSuffix))
                key = $"{key}_{DateTime.Now.ToString(_listNameSuffix)}";
            return database.ListRightPushAsync(key, new RedisValue[] { content });
        }

        public void Dispose()
        {
            if (redis == null)
            {
                return;
            }

            redis.Close(false);
            redis.Dispose();
        }
    }
}
