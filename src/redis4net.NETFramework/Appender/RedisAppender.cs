using System;
using log4net.Appender;
using log4net.Core;

namespace redis4net.Appender
{
    using redis4net.Redis;
    using System.Reflection;

    public class RedisAppender : AppenderSkeleton
    {
        protected ConnectionFactory ConnectionFactory { get; set; }
        public string RemoteAddress { get; set; }
        public int RemotePort { get; set; }
        public string ListName { get; set; }
        public string Password { get; set; }
        public string ListNameSuffix { get; set; }
        public int Database { get; set; }
        /// <summary>
        /// Assembly qualified class name that contains settings method specified below. Which basically return 'connectionString' value
        /// </summary>
        public string SettingsClassName { get; set; }
        /// <summary>
        /// Settings method should be defined in settingsClass. It should be public, static, does not take any parameters and should have a return type of 'String', which is basically 'connectionString' value.
        /// </summary>
        public string SettingsMethodName { get; set; }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            InitializeConnectionFactory();
        }

        protected virtual void InitializeConnectionFactory()
        {
            string connStr = null;
            if (!string.IsNullOrEmpty(SettingsClassName) && !string.IsNullOrEmpty(SettingsMethodName))
            {
                if (!SettingsClassName.Contains(","))
                    throw new Exception("Redis connection string class name setting error, should be 'ClassTypeFullName,AssemblyName'.");

                Assembly assembly = Assembly.Load(SettingsClassName.Split(',')[1].Trim());
                Type type = assembly.GetType(SettingsClassName.Split(',')[0].Trim());
                connStr = type.InvokeMember(SettingsMethodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null)?.ToString();
            }

            var connection = new Connection();
            ConnectionFactory = new ConnectionFactory(connection, RemoteAddress, RemotePort, 1
                                                    , ListName, Password, ListNameSuffix, Database
                                                    , connStr);
        }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            try
            {
                var message = RenderLoggingEvent(loggingEvent);
                ConnectionFactory.GetConnection().AddToList(message);
            }
            catch (Exception exception)
            {
                ErrorHandler.Error("Unable to send logging event to remote redis host " + RemoteAddress + " on port " + RemotePort, exception, ErrorCode.WriteFailure);
            }
        }
    }
}
