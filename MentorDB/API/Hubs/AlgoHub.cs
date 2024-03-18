using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Services;

namespace API.Hubs {
    public class AlgoHub : Hub {
        private readonly HubsService _service;

        public AlgoHub(HubsService service) {
            _service = service;
        }

        public async void SendMessage(string to, string message) {
            Clients.All.SendAsync("ReceiveMessage", Context.User.Identity.Name, message);  
        }
    }
}
