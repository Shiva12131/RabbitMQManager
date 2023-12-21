using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using ManagerRabbitMQ_POC.Model;
using Newtonsoft.Json;

namespace ManagerRabbitMQ_POC
{
    public class RpcClient : IDisposable
    {
        private const string QUEUE_NAME = "rpc_queue";

        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

        public RpcClient()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            // declare a server-named queue
            replyQueueName = channel.QueueDeclare().QueueName;
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                    return;
                var body = ea.Body.ToArray();

                //Changes made here
                ResponseModel? responseModel = null;
                try
                {
                    var response = Encoding.UTF8.GetString(body);

                    responseModel = JsonConvert.DeserializeObject<ResponseModel>(response);

                    Console.WriteLine("FilePath:-" + responseModel!.filePath);
                    Console.WriteLine("Machine Name: " + responseModel.MachineName);
                    tcs.TrySetResult(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: in getting the response from the server");
                }
                 
                
            };

            channel.BasicConsume(consumer: consumer,
                                 queue: replyQueueName,
                                 autoAck: true);
        }

        public Task<string> CallAsync(RequestModel requestModel, CancellationToken cancellationToken = default)
        {
            IBasicProperties props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            var requestObj = JsonConvert.SerializeObject(requestModel);
            var messageBytes = Encoding.UTF8.GetBytes(requestObj);
            var tcs = new TaskCompletionSource<string>();
            callbackMapper.TryAdd(correlationId, tcs);

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: QUEUE_NAME,
                                 basicProperties: props,
                                 body: messageBytes);

            cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
            return tcs.Task;
        }

        public void Dispose()
        {
            channel.Close();
            connection.Close();
        }
    }
}
