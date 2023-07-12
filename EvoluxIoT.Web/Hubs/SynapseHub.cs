using Microsoft.AspNetCore.SignalR;

namespace EvoluxIoT.Web.Hubs
{
    public class SynapseHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            bool? is_authenticated = Context.User?.Identity?.IsAuthenticated;

            if (is_authenticated != true)
            {
                Context.Abort();
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.Name);
            await base.OnConnectedAsync();
        }





    }
}
