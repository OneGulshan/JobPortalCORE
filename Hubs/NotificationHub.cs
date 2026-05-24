using Microsoft.AspNetCore.SignalR;

namespace JobPortalCORE.Hubs
{
    // Ye class Hub se inherit honi chahiye
    public class NotificationHub : Hub
    {
        // Yahan hume custom method likhne ki zaroorat nahi, 
        // hum seedha Controller se client ko message bhejenge.
    }
}