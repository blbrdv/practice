using System.Net;
using System.Net.Sockets;
using System.Text;
using Common;
using Newtonsoft.Json;

namespace PrimeTime;

public static class Server
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
            var stream = client.GetStream();
            var buffer = new byte[8192];

            while (client.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer);
                var requestString = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"{id} | Received | {requestString} |");

                try
                {
                    dynamic request = JsonConvert.DeserializeObject(requestString)!;

                    if (request.method != "isPrime")
                    {
                        var malformed = await SendMalformedResponseAsync(stream);
                        Console.WriteLine($"{id} | Send | {malformed} |");
                    }

                    int number = Convert.ToInt32(request.number);
                    var isPrime = CheckIfPrime(number);

                    var result = await PrepareJson(stream, isPrime);
                    Console.WriteLine($"{id} | Send | {result} |");
                }
                catch (Exception)
                {
                    var malformed = await SendMalformedResponseAsync(stream);
                    Console.WriteLine($"{id} | Send | {malformed} |");
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

    // https://stackoverflow.com/a/15743238
    private static bool CheckIfPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        var boundary = (int)Math.Floor(Math.Sqrt(number));
          
        for (var i = 3; i <= boundary; i += 2)
            if (number % i == 0)
                return false;
    
        return true;   
    }

    private static async Task<string> PrepareJson(Stream stream, dynamic value)
    {
        dynamic response = new { method = "isPrime", prime = value };
        string responseString = JsonConvert.SerializeObject(response);
        var responseBytes = Encoding.UTF8.GetBytes(responseString + "\n");
        await stream.WriteAsync(responseBytes);
        return responseString;
    }

    private static async Task<string> SendMalformedResponseAsync(Stream stream)
    {
        return await PrepareJson(stream, "malformed");
    }
}