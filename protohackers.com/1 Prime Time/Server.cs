using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO.Pipelines;
using Common;
using Newtonsoft.Json;

namespace PrimeTime;

public static class Server
{
    private const int Port = 5003;
    
    internal static async Task Run()
    {
        var listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 5003);
        listener.Start();
        Console.WriteLine($"\"Prime Time\" server started on port {Port}");

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

    private static async Task HandleConnectionAsync(TcpClient client)
    {
        var id = Id.New();
        
        Console.WriteLine($"{id} | Connected |");

        try
        {
            await using var stream = client.GetStream();
            var reader = PipeReader.Create(stream);

            var malformed = false;
            while (!malformed)
            {
                
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;

                while (!malformed && TryReadLine(ref buffer, out var line))
                {
                    try
                    {
                        Console.WriteLine($"{id} | Received | {line} |");
                        
                        var settings = new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            Converters = { new DoubleConverter() },
                            ContractResolver = new NonNullablePropertiesRequiredResolver()
                        };
                        var request = JsonConvert.DeserializeObject<Payload>(line, settings)!;
                        
                        if (request.Method == "isPrime")
                        {
                            var resultMsg = await SendResponse(stream, CheckIfPrime(request.Number));
                            Console.WriteLine($"{id} | Send | {resultMsg} |");
                        }
                        else
                        {
                            malformed = true;
                            var malformedMsg = await SendMalformedResponse(stream);
                            Console.WriteLine($"{id} | Send | {malformedMsg} |");
                        }
                    }
                    catch (Exception e)
                    {
                        malformed = true;
                        var malformedMsg = await SendMalformedResponse(stream);
                        Console.WriteLine($"{id} | Send | {malformedMsg} |");
                        
                        // Console.WriteLine($"{id} | Error | {e.Message} |");
                        // if (e.StackTrace != null)
                        //     Console.WriteLine(e.StackTrace);
                    }
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (malformed || result.IsCompleted)
                {
                    break;
                }
            }

            if (malformed)
            {
                var malformedMsg = await SendMalformedResponse(stream);
                Console.WriteLine($"{id} | Send | {malformedMsg} |");
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
            Console.WriteLine($"{id} | Disconnected |");
            client.Close();
        }
    }

    private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out string line)
    {
        var position = buffer.PositionOf((byte)'\n');
        if (position == null)
        {
            line = "";
            return false;
        }

        line = Encoding.UTF8.GetString(buffer.Slice(0, position.Value));
        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
        return true;
    }

    // https://stackoverflow.com/a/15743238
    private static bool CheckIfPrime(double number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        var boundary = Math.Floor(Math.Sqrt(number));
          
        for (var i = 3L; i <= boundary; i += 2)
            if (number % i == 0)
                return false;
    
        return true;   
    }

    private static async Task<string> SendResponse(Stream stream, bool value)
    { 
        return await SendJson(stream, new { method = "isPrime", prime = value });
    }

    private static async Task<string> SendMalformedResponse(Stream stream)
    {
        return await SendJson(stream, new { malformed = true });
    }

    private static async Task<string> SendJson(Stream stream, dynamic response)
    {
        string responseString = JsonConvert.SerializeObject(response);
        var responseBytes = Encoding.UTF8.GetBytes(responseString + "\n");
        await stream.WriteAsync(responseBytes);
        return responseString;
    }
}