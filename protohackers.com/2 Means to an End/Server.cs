using System.Net;
using System.Net.Sockets;
using Common;

namespace MeansToAnEnd;

internal static class Server
{
    private const int Port = 5003;

    internal static async Task Run()
    {
        var listener = new TcpListener(IPAddress.Parse("0.0.0.0"), Port);
        listener.Start();
        Console.WriteLine($"\"Means to an End\" server started on port {Port}");

        while (true)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleConnectionAsync(client));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    
    private static Task HandleConnectionAsync(TcpClient client)
    {
        var id = Id.New();
        
        Console.WriteLine($"{id} | Connected |");

        try
        {
            var data = new List<(int timestamp, int price)>();
            
            var stream = client.GetStream();
            BinaryReader reader = new(stream);
            BinaryWriter writer = new(stream);

            while (client.Connected)
            {
                try
                {
                    var type = reader.ReadChar();
                    var first = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray());
                    var second = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray());

                    if (type == 'I')
                    {
                        Console.WriteLine($"{id} | Received | I {first} {second} |");
                        data.Add((first, second));
                    } 
                    else if (type == 'Q')
                    {
                        Console.WriteLine($"{id} | Received | Q {first} {second} |");

                        int result;
                        if (first > second) 
                            result = 0;
                        else
                        {
                            result = (int)data
                                .Where(price => first <= price.timestamp && price.timestamp <= second)
                                .Select(price => price.price)
                                .DefaultIfEmpty(0)
                                .Average();
                        }
                        var response = BitConverter.GetBytes(result).Reverse().ToArray();
                        
                        Console.WriteLine($"{id} | Send | {result} |");
                        writer.Write(response);
                    }
                    else
                    {
                        Console.WriteLine($"{id} | Malformed request |");
                        break;
                    }
                }
                catch (EndOfStreamException)
                {
                    Console.WriteLine($"{id} | Disconnected |");
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{id} | Error | {e.Message} |");
            if (e.StackTrace != null)
                Console.WriteLine(e.StackTrace);
        }
        finally
        {
            client.Close();
        }

        return Task.CompletedTask;
    }
}