using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO.Pipelines;
using System.Text.Json;
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
                        
                        dynamic request = JsonConvert.DeserializeObject(line)!;
                    
                        if (request.method != "isPrime")
                        {
                            malformed = true;
                            var malformedMsg = await SendMalformedResponseAsync(stream);
                            Console.WriteLine($"{id} | Send | {malformedMsg} |");
                        }
                    
                        int number = Convert.ToInt32(request.number);
                        var isPrime = CheckIfPrime(number);
                    
                        var resultMsg = await PrepareJson(stream, isPrime);
                        Console.WriteLine($"{id} | Send | {resultMsg} |");
                    }
                    catch (Exception)
                    {
                        malformed = true;
                        var malformedMsg = await SendMalformedResponseAsync(stream);
                        Console.WriteLine($"{id} | Send | {malformedMsg} |");
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
                var malformedMsg = await SendMalformedResponseAsync(stream);
                Console.WriteLine($"{id} | Send | {malformedMsg} |");
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
    private static bool CheckIfPrime(long number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        var boundary = (long)Math.Floor(Math.Sqrt(number));
          
        for (var i = 3L; i <= boundary; i += 2)
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