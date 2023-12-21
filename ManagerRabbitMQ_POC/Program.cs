using ManagerRabbitMQ_POC;
using ManagerRabbitMQ_POC.Model;

public class Rpc
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("RPC Client");
       

        RequestModel? requestModel = new RequestModel()
        {
            ID = 1,URL="www.newswalla.co.in",MachineName="Reuest_1",CreatedTime= DateTime.Now,RequestTime= DateTime.Now
        };
       
        await InvokeAsync(requestModel);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    private static async Task InvokeAsync(RequestModel requestModel)
    {
        using var rpcClient = new RpcClient();

        Console.WriteLine(" [x] Requesting to get relevant data)", requestModel);
        var response = await rpcClient.CallAsync(requestModel);
        Console.WriteLine(" [.] Got '{0}'", response);
    }
}