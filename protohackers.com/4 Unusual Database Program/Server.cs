using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Common;

namespace UnusualDatabaseProgram;

internal static class Server
{
    private const int Port = 5003;
    
    private static readonly Dictionary<string, string> Data = new()
    {
        { "version", "Unusual Database Program 1.0" }
    };
    
    internal static async Task Run()
    {
        var listener = new UdpClient(Port);
        
        Console.WriteLine($"\"Unusual Database Program\" server started on port {Port}");
        
        try
        {
            while (true)
            {
                var result = await listener.ReceiveAsync();
                _ = HandleConnection(listener, result);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            listener.Close();
        }
    }

    private static async Task HandleConnection(UdpClient client, UdpReceiveResult result)
    {
        var id = Id.New();
        
        Console.WriteLine($"{id} | Connected |");

        try
        {
            var message = Encoding.ASCII.GetString(result.Buffer);
        
            Console.WriteLine($"{id} | Received | {message} |");

            if (message.Contains("="))
            {
                var resultMessage = Regex.Matches(message, @"([^=]*)=((?>.|\n)*)", RegexOptions.IgnoreCase);
                var key = resultMessage[0].Groups[1].Value;
                var value = resultMessage[0].Groups[2].Value;

                if ("version".Equals(key))
                    return;

                Data[key] = value;
            }
            else
            {
                var value = Data.GetValueOrDefault(message) ?? string.Empty;
                var resultMessage = $"{message}={value}";
                var resultBytes = Encoding.ASCII.GetBytes(resultMessage);
                await client.SendAsync(resultBytes, result.RemoteEndPoint);
                
                Console.WriteLine($"{id} | Sending | {resultMessage} |");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{id} | Error | {e.Message} |");
            if (e.StackTrace != null)
                Console.WriteLine(e.StackTrace);
        }
    }
}