using System.Net;
using System.Net.Sockets;

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
        Console.WriteLine("Client connected");

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
                        data.Add((first, second));
                    } 
                    else if (type == 'Q')
                    {
                        var prices = data
                            .Where(price => first <= price.timestamp && price.timestamp <= second)
                            .Select(price => price.price);
                        
                        var result = prices.Any() ? Convert.ToInt32(Math.Round(prices.Average())) : 0;
                        var response = BitConverter.GetBytes(result).Reverse().ToArray();
                        
                        writer.Write(response);
                        Console.WriteLine(result);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            client.Close();
        }
    }
}