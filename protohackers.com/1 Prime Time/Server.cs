using System.Net;
using System.Net.Sockets;
using System.Text;
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
        Console.WriteLine("Client connected");

        try
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];

            while (client.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer);
                var requestString = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Received request: " + requestString);

                try
                {
                    dynamic request = JsonConvert.DeserializeObject(requestString)!;

                    if (request.method != "isPrime")
                        await SendMalformedResponseAsync(stream);

                    int number = Convert.ToInt32(request.number);
                    var isPrime = CheckIfPrime(number);

                    await PrepareJson(stream, isPrime);
                }
                catch (Exception)
                {
                    await SendMalformedResponseAsync(stream);
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

    private static async Task PrepareJson(Stream stream, dynamic value)
    {
        dynamic response = new { method = "isPrime", prime = value };
        string responseString = JsonConvert.SerializeObject(response);
        var responseBytes = Encoding.UTF8.GetBytes(responseString + "\n");
        await stream.WriteAsync(responseBytes);
        
        Console.WriteLine("Response send: " + responseString);
    }

    private static async Task SendMalformedResponseAsync(Stream stream)
    {
        await PrepareJson(stream, "malformed");
    }
}