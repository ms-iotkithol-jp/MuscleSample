using Microsoft.Azure.Devices.Client;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace UWPArduino2017
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            usbConnection = new UsbSerial("VID_2A03", "PID_0043");
            arduino = new RemoteDevice(usbConnection);
            arduino.DeviceReady += Arduino_DeviceReady;
            usbConnection.begin(57600, SerialConfig.SERIAL_8N1);
        }

        private void Arduino_DeviceReady()
        {
            arduinoReady = true;
            for (int i = 0; i < 6; i++)
            {
                arduino.pinMode("A" + i, PinMode.ANALOG);
            }
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,() =>
            {
                this.buttonStart.Foreground = new SolidColorBrush(Colors.Azure);
            });
        }

        RemoteDevice arduino;
        IStream usbConnection;
        bool arduinoReady = false;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // IoT Hubに接続するための接続先とセキュリティ情報
            string cs = "<< Azure IoT Hub Connection String for Device Id >>";
            var client = DeviceClient.CreateFromConnectionString(cs, TransportType.Mqtt);
            await client.OpenAsync();
            await client.SetMethodHandlerAsync("Alert", AlertMethodHandler, client);

            for (int i = 0; i < 2000; i++)
            {
                var data = new
                {
                    MeasuredTime = DateTime.Now,
                    Muscle = arduino.analogRead("A1")
                };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var msg = new Message(Encoding.UTF8.GetBytes(json));
                await client.SendEventAsync(msg);
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            await client.CloseAsync();

        }

        private Task<MethodResponse> AlertMethodHandler(MethodRequest methodRequest, object userContext)
        {
            Debug.WriteLine("Method Called - " + methodRequest.Name + " with " + methodRequest.DataAsJson);
            return new Task<MethodResponse>(() =>
            {
                return new MethodResponse(0);
            });
        }

        private async void ReceiveMessagesAsync(DeviceClient client)
        {
            while (true)
            {
                var message = await client.ReceiveAsync();
                var content = Encoding.UTF8.GetString(message.GetBytes());
                await client.CompleteAsync(message);
            }
        }

    }
}
