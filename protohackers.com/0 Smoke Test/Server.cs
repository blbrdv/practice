using System.Net;
using System.Net.Sockets;
using Common;

namespace SmokeTest;

internal static class Server
{

    internal static async Task Run()
    {
        var listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 5003);
        listener.Start();

        Console.WriteLine("Server started.");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();

            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        var id = Id.New();
        Console.WriteLine($"{id} | Connected |");
        
        var stream = client.GetStream();

        var buffer = new byte[4096];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
        }
        
        Console.WriteLine($"{id} | Disconnected |");
        client.Close();
    }
    
}