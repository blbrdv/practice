using System.Net;
using System.Net.Sockets;
using Common;

namespace MeansToAnEnd;

internal static class Server
{

    internal static async Task Run()
    {
        var listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 5003);
        listener.Start();
        Console.WriteLine("Server started");

        while (true)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleConnectionAsync(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    
    private static async Task HandleConnectionAsync(TcpClient client)
    {
        var id = Id.New();
        
        Console.WriteLine($"{id} | Connected |");

        try
        {
            var data = new List<(int timestamp, int price)>();
            
            var stream = client.GetStream();
            var reader = new BinaryReader(stream);
            var writer = new BinaryWriter(stream);

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
                        var prices = data
                            .Where(price => first <= price.timestamp && price.timestamp <= second)
                            .Select(price => price.price);
                        
                        var result = prices.Any() ? Convert.ToInt32(Math.Round(prices.Average())) : 0;
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
        }
        finally
        {
            Console.WriteLine($"{id} | Disconnected |");
            client.Close();
        }
    }
}