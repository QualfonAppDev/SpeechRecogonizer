using Microsoft.AspNet.SignalR.Client;
namespace SpeechRecogonizer.hubClient
{
    internal class SignalRClient
    {
        private readonly HubConnection _connection;
        private readonly IHubProxy _hubProxy;
        string serverUrl = "https://qscriptwebapi-dev.qualfon.com";
        string hubName = "CommonHub";

        public SignalRClient()
        {

            // Initialize the SignalR connection
            _connection = new HubConnection(serverUrl);
            // Create a proxy for the specified hub
            _hubProxy = _connection.CreateHubProxy(hubName);

            // Register server-side event handlers
            RegisterServerHandlers();
        }

        // Connect to the SignalR server
        public async Task ConnectAsync()
        {
            try
            {
                await _connection.Start();
                Console.WriteLine("Connected to SignalR server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to SignalR server: {ex.Message}");
            }
        }

        // Disconnect from the SignalR server
        public void Disconnect()
        {
            try
            {
                _connection.Stop();
                Console.WriteLine("Disconnected from SignalR server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting from SignalR server: {ex.Message}");
            }
        }

        // Send a message to the SignalR server
        public async Task SendMessageAsync(string methodName, params object[] args)
        {
            try
            {
                await _hubProxy.Invoke(methodName, args);
              //  Console.WriteLine($"Message sent to method '{methodName}' with arguments: {string.Join(", ", args)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        // Register handlers for events sent by the server
        private void RegisterServerHandlers()
        {
            //_hubProxy.On<string, string>("ReceiveMessage", (user, message) =>
            //{
            //    Console.WriteLine($"Received message from {user}: {message}");
            //});
        }
    }
}
