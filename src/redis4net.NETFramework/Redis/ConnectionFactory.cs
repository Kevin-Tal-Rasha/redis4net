using System;

namespace redis4net.Redis
{
    using System.Reflection;
    using System.Threading;

    public class ConnectionFactory : IConnectionFactory
    {
        private static readonly object Lock = new object();

        private readonly string _hostname;
        private readonly int _portNumber;
        private readonly string _password;
        private readonly string _listNameSuffix;
        private readonly int _database;
        private readonly int _failedConnectionRetryTimeoutInSeconds;
        private readonly string _listName;
        private static string _connectionString;
        private readonly IConnection _connection;

        public ConnectionFactory(IConnection connection, string hostName, int portNumber
                               , int failedConnectionRetryTimeoutInSeconds, string listName
                               , string password
                               , string listNameSuffix, int database
                               , string connectionString)
        {
            _connection = connection;

            _hostname = hostName;
            _portNumber = portNumber;
            _failedConnectionRetryTimeoutInSeconds = failedConnectionRetryTimeoutInSeconds;
            _listName = listName;
            _password = password;
            _listNameSuffix = listNameSuffix;
            _database = database;
            _connectionString = connectionString;
        }

        public IConnection GetConnection()
        {
            InitializeConnection();
            return _connection;
        }

        private void InitializeConnection()
        {
            if (_connection.IsOpen())
            {
                return;
            }

            lock (Lock)
            {
                try
                {
                    OpenConnection();

                    if (!_connection.IsOpen())
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(_failedConnectionRetryTimeoutInSeconds));
                        OpenConnection();
                    }
                }
                catch
                {
                    // Nothing to do if this fails
                }
            }
        }

        private void OpenConnection()
        {
            if (!_connection.IsOpen())
            {
                if (!string.IsNullOrEmpty(_connectionString))
                    _connection.Open(_connectionString, _listName, _listNameSuffix);
                else
                    _connection.Open(_hostname, _portNumber, _listName, _password, _listNameSuffix, _database);
            }
        }
    }
}
