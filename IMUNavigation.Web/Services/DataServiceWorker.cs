using VNSDK;
using VNSDK.Registers.System;

namespace IMUNavigation.Web.Services;

using Microsoft.AspNetCore.SignalR;

public class DataServiceWorker(ILogger<DataServiceWorker> logger, IHubContext<DataHub> dataHub)
    : BackgroundService
{
    // Define the port connection parameters to be used later
    private const string Port = "COM6";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(4000, stoppingToken);
        
        var sensor = new Sensor();

        // Connect to the sensor with the specified baud rate
        sensor.AutoConnect(Port);

        // Verify that the sensor is connected
        if (!sensor.VerifySensorConnectivity())
        {
            logger.LogError("Error: Failed to connect to sensor");
            return;
        }

        // Get the sensor's connection information
        logger.LogInformation("Connected to {Port} at {ConnectedBaudRate}", Port, sensor.ConnectedBaudRate());

        // Get the sensor's model number
        var modelReg = new Model();
        sensor.ReadRegister(modelReg);

        logger.LogInformation("Model Number: {ModelNumber}", modelReg.model);

        var binaryOutput1 = new BinaryOutput1();
        /*
            Sets the fixed rate at which the message is sent out the
            selected serial port(s). The number given is a divisor of
            the ImuRate which is nominally 800Hz. For example to
            have the sensor output at 50Hz you would set the
            Divisor equal to 16.
         */
        binaryOutput1.rateDivisor = 50; // 1 Hz on all but VN-300
        binaryOutput1.asyncMode.serial1 = true;
        binaryOutput1.asyncMode.serial2 = true;
        binaryOutput1.common.timeStartup = true;
        binaryOutput1.common.deltas = true;

        // The Register base object is also used when identifying measurements
        sensor.WriteRegister(binaryOutput1);

        while (!stoppingToken.IsCancellationRequested)
        {
            var measurement = sensor.GetNextMeasurement();

            if (!measurement.HasValue) continue;

            if (!measurement.Value.MatchesMessage(binaryOutput1)) continue;

            var timestamp = DateTime.UtcNow;

            if (measurement.Value.imu is not { deltaTheta: not null, deltaVel: not null }) continue;

            await dataHub.Clients.All.SendAsync("ReceiveData", timestamp,
                measurement.Value.imu.deltaTheta?.deltaTheta.x, measurement.Value.imu.deltaTheta?.deltaTheta.y,
                measurement.Value.imu.deltaTheta?.deltaTheta.z, measurement.Value.imu.deltaVel?.x,
                measurement.Value.imu.deltaVel?.y, measurement.Value.imu.deltaVel?.z, cancellationToken: stoppingToken);
        }

        sensor.Disconnect();
    }
}