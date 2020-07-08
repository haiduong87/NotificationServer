using System.Collections.Generic;
using NATS.Client;

namespace NotificationServer.Miscellaneous
{
    public class NatsConnectionPool
    {
        private readonly LinkedList<IConnection> _connections;
        private readonly bool _useRoundRobbin;

        private LinkedListNode<IConnection> _connetionNode;

        /// <exception cref="T:NATS.Client.NATSNoServersException">No connection to a NATS Server could be established.</exception>
        /// <exception cref="T:NATS.Client.NATSConnectionException">
        ///     <para>A timeout occurred connecting to a NATS Server.</para>
        ///     <para>-or-</para>
        ///     <para>
        ///         An exception was encountered while connecting to a NATS Server. See
        ///         <see cref="P:System.Exception.InnerException" /> for more
        ///         details.
        ///     </para>
        /// </exception>
        public NatsConnectionPool(Configuration configuration)
        {
            _useRoundRobbin = configuration.UseConnectionPool;

            var cf = new ConnectionFactory();
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.AllowReconnect = true;
            opts.MaxReconnect = Options.ReconnectForever;

            _connections = new LinkedList<IConnection>();
            if (_useRoundRobbin)
            {
                for (var i = 0; i < configuration.NatsConnectionPoolSize; i++)
                    foreach (var serverConfig in configuration.NatsServers)
                    {
                        opts.Servers = serverConfig.Split(',');
                        _connections.AddLast(cf.CreateConnection(opts));
                    }
            }
            else
            {
                opts.Servers = configuration.NatsServers[0].Split(',');
                _connections.AddLast(cf.CreateConnection(opts));
            }
        }

        public IConnection Connection
        {
            get
            {
                _connetionNode ??= _connections.First;
                if (_useRoundRobbin)
                    _connetionNode = _connetionNode.Next ?? _connections.First;

                // ReSharper disable once PossibleNullReferenceException
                return _connetionNode.Value;
            }
        }
    }
}