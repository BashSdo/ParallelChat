using System.IO.Pipes;

class Program
{
    static async Task Main()
    {
        Server myServer = new Server();
        await myServer.Start("testpipe", 100);
    }
}

class Server
{
    private List<Client> clients = [];

    public async Task Start(string pipeName, int maxClients)
    {
        while (true) { 
            await AcceptConnection(pipeName, maxClients);
        }
    }

    private async Task AcceptConnection(string pipeName, int maxClients)
    {
        var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, maxClients, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        pipe.WaitForConnection();

        var client = new Client(pipe);
        clients.Add(client);

        await Broadcast("Client connected.");

        new Task(async () =>
        {
            while (client.IsConnected)
            {
                var msg = await client.ReadMessage();
                if (msg != null)
                {
                    Console.WriteLine(msg);
                    await Broadcast(msg);
                }
            }

            clients.Remove(client);
            await Broadcast("Client disconnected.");
        }).Start();
    }

    private async Task Broadcast(string msg)
    {
        Console.WriteLine($"Broadcast: {msg}");
        foreach (var client in clients)
        {
            if (client.IsConnected)
            {
                await client.SendMessage(msg);
            }
        }
    }
}

class Client : IDisposable
{
    private NamedPipeServerStream pipe;
    private StreamReader sr;
    private StreamWriter sw;

    public bool IsConnected
    {
        get
        {
            return pipe.IsConnected;
        }
    }

    public Client(NamedPipeServerStream pipe)
    {
        this.pipe = pipe;
        sr = new StreamReader(pipe);
        sw = new StreamWriter(pipe);
        sw.AutoFlush = true;
    }

    public async Task SendMessage(string msg)
    {
        await sw.WriteLineAsync(msg);
    }

    public async Task<string?> ReadMessage()
    {
        return await sr.ReadLineAsync();
    }

    public void Dispose()
    {
        sr.Dispose();
        sw.Dispose();
    }
}