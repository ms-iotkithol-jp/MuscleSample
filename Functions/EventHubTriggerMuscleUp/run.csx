using System;

public static void Run(string myEventHubMessage, TraceWriter log)
{
    log.Info($"C# Event Hub trigger function processed a message: {myEventHubMessage}");

    var cs ="<< Azure Iot Hub Service Role Connection String >>";
    // Azure IoT HubにServiceロールで接続
    var client = Microsoft.Azure.Devices.ServiceClient.CreateFromConnectionString(cs);
    dynamic msgJson = Newtonsoft.Json.JsonConvert.DeserializeObject(myEventHubMessage);
    dynamic deviceIdToken= msgJson.SelectToken("deviceid");
    string deviceId = deviceIdToken.Value;
    log.Info($"Got deviceId:{deviceId}");
    dynamic muscleAvgToken = msgJson.SelectToken("muscleavg");
    double muscleAvg = muscleAvgToken.Value;
    log.Info($"Got muscleAvg:{muscleAvg}");
    var notifyMessage = new
    {
        targetDevice = deviceId,
        muscleAvg = muscleAvg
    };

    string noticeContent = Newtonsoft.Json.JsonConvert.SerializeObject(notifyMessage);
    var noticeMessage = new Microsoft.Azure.Devices.Message(System.Text.Encoding.UTF8.GetBytes(noticeContent));

    // IoT Hubを介して、デバイスにメッセージ送信
    client.SendAsync("deviceRaspberryPi", noticeMessage);

    var hubConnection = new Microsoft.AspNet.SignalR.Client.HubConnection("http://[Web App Name].azurewebsites.net/");
    var proxy = hubConnection.CreateHubProxy("MuscleHub");
    hubConnection.Start().Wait();
    dynamic endtimeToken = msgJson.SelectToken("endtime");
    DateTime endtime = endtimeToken.Value;
  //  DateTime time = DateTime.Parse(endtime);
    proxy.Invoke("Update",new {muscle=muscleAvg, measuredTime=endtime});
    log.Info("Notify Done.");
}