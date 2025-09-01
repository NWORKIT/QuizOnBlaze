using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace QuizOnBlaze.Services
{
    public class HubConnectionPlayerService
    {
        public HubConnection? Connection { get; private set; }
        private bool _initialized = false;

        public async Task StartAsync(NavigationManager nav, Guid playerId)
        {
            if (_initialized)
                return;

            Connection = new HubConnectionBuilder()
                .WithUrl(nav.ToAbsoluteUri($"/quizhub?playerId={playerId}"))
                .WithAutomaticReconnect()
                .Build();

            await Connection.StartAsync(); // Connection start
            _initialized = true;
        }
    }
}
