const connection = new signalR.HubConnectionBuilder().withUrl("/hubs/data").configureLogging(signalR.LogLevel.Information).build();
let count = 0;
connection.on("ReceiveData", (utcDateTime, deltaThetaX, deltaThetaY, deltaThetaZ, deltaVelX, deltaVelY, deltaVelZ) => {
    
    const table = document.getElementById("data");
    const row = table.insertRow(1);
    row.setAttribute("scope", "row");
    row.insertCell(0).innerHTML = (++count).toString();
    row.insertCell(1).innerHTML = moment(utcDateTime).local().format("YYYY-MM-DD HH:mm:ss.SSS");
    row.insertCell(2).innerHTML = deltaThetaX;
    row.insertCell(3).innerHTML = deltaThetaY;
    row.insertCell(4).innerHTML = deltaThetaZ;
    row.insertCell(5).innerHTML = deltaVelX;
    row.insertCell(6).innerHTML = deltaVelY;
    row.insertCell(7).innerHTML = deltaVelZ;
});

connection.start()
    .then(() => {
        console.log("SignalR connection started successfully.");
    })
    .catch((err) => {
        console.error("Error starting SignalR connection: " + err);
    });