using Microsoft.AspNetCore.SignalR;

namespace IMUNavigation.Web.Services;

public class DataHub : Hub
{
    public async Task SendDataAsync(DateTime utcDateTime, float? deltaThetaX, float? deltaThetaY, float? deltaThetaZ, float? deltaVelX, float? deltaVelY, float? deltaVelZ)
    {
        await Clients.All.SendAsync("ReceiveData", utcDateTime, deltaThetaX, deltaThetaY, deltaThetaZ, deltaVelX, deltaVelY, deltaVelZ);
    }
}