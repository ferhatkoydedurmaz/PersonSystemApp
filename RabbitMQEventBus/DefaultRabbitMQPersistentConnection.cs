using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace RabbitMQEventBus;

public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection _connection;
    private readonly int _retryCount;
    private bool _disposed;
    private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
    private object LockObject = new();

    public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount, ILogger<DefaultRabbitMQPersistentConnection> logger)
    {
        _connectionFactory=connectionFactory;
        _logger=logger;
        _retryCount=retryCount;
    }

    public bool IsConnected => _connection != null && _connection.IsOpen && _disposed == false;

    public IModel CreateModel()
    {
        if (IsConnected == false)
        {
            throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
        }

        return _connection.CreateModel();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _connection.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex.ToString());
        }
    }

    public bool TryConnect()
    {
        lock (LockObject)
        {
            _logger.LogInformation("RabbitMQ Client is trying to connect");
            var policy = RetryPolicy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex.ToString());
                });

            policy.Execute(() => { _connection = _connectionFactory.CreateConnection(); });

            if (IsConnected)
            {
                _connection.ConnectionShutdown += OnConnectionShutDown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                _logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");
                return true;
            }
            else
            {
                _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
                return false;
            }
        }
    }

    private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
    {
        if (_disposed) return;
        _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

        TryConnect();
    }

    private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        if (_disposed) return;
        _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
        TryConnect();
    }

    private void OnConnectionShutDown(object? sender, ShutdownEventArgs e)
    {
        if (_disposed) return;
        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
        TryConnect();
    }
}