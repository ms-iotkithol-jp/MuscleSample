using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WebAppMuscleViewer.Hubs
{
    public class MuscleHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }

        [Microsoft.AspNet.SignalR.Hubs.HubMethodName("update")]
        public void Update(Models.MuscleMessage packet)
        {
            Clients.Others.Update(packet);
        }
    }
}