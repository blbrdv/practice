using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Common;

namespace BudgetChat;

internal static class Server
{
    private const int Port = 5003;
    
    private static readonly Dictionary<string, StreamWriter> Clients = new();
    
    internal static async Task Run()
    {
        var listener = new TcpListener(IPAddress.Parse("0.0.0.0"), Port);
        listener.Start();
        Console.WriteLine($"\"Budget Chat\" server started on port {Port}");
        
        while (true)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleConnection(client));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private static async Task HandleConnection(TcpClient client)
    {
        var id = Id.New();
        
        Console.WriteLine($"{id} | Connected |");
        
        try
        {
            await using var stream = client.GetStream();
            await using StreamWriter writer = new(stream);
            using StreamReader reader = new(stream);

            string name;
            Regex rg = new(@"^[a-zA-Z0-9]{1,16}$");
            
            try
            {
                await Send(writer, "Welcome to budgetchat! What shall I call you?");
                Console.WriteLine($"{id} | Welcome message sent |");
                
                var line = await reader.ReadLineAsync();
                var proposedName = line?.Trim() ?? "";

                if (rg.IsMatch(proposedName))
                {
                    name = proposedName;
                    Console.WriteLine($"{id} | Name set | {name} |");
                }
                else
                {
                    return;
                }
                
                var clientNames = Clients.Any() ? string.Join(", ", Clients.Keys) : string.Empty;
                await Send(writer, $"* The room contains: {clientNames}");
                Console.WriteLine($"{id} | Room | {clientNames} |");
                
                Clients.Add(name, writer);
                _ = SendToAll($"* {name} has entered the room", name);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{id} | Error | {e.Message} |");
                return;
            }

            while (client.Connected)
            {
                try
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null)
                    {
                        DisconnectClient(name);
                        break;
                    }

                    _ = SendToAll($"[{name}] {line}", name);
                    Console.WriteLine($"{id}-{name} | Send | {line}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{id} | Error | {e.Message} |");
                    DisconnectClient(name);
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
            client.Close();
        }

        Console.WriteLine($"{id} | Disconnected |");
    }

    private static async Task Send(TextWriter writer, string message)
    {
        await writer.WriteLineAsync(message);
        await writer.FlushAsync();
    }

    private static async Task SendToAll(string message, string except)
    {
        if (!Clients.Any())
            return;
        
        var recipients = Clients.Where(client => client.Key != except);
        var writers = recipients.Select(client => client.Value);
        foreach (var writer in writers)
            await Send(writer, message);
    }
    
    private static void DisconnectClient(string name)
    {
        _ = SendToAll($"* {name} has left the room", name);
        Clients.Remove(name);
    }
}