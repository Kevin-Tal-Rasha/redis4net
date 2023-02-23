using System.Threading.Tasks;

namespace redis4net.Redis
{
    public interface IConnection
    {
        void Open(string hostname, int port, string listName, string password, string listNameSuffix, int database);
        void Open(string connString, string listName, string listNameSuffix);
        bool IsOpen();
        Task<long> AddToList(string content);
    }
}
