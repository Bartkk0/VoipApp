using System.Net;

internal class Program {
    public static void Main(string[] args) {
        var server = new Server.Server(IPAddress.Any, 3621);
        server.Start();
    }
}