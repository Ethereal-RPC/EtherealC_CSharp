using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherealC.Net.NetNodeClient.Model;
using EtherealC.Request.WebSocket;

namespace EtherealC.Net.NetNodeClient.Request
{
    public class ServerNetNodeRequest:WebSocketRequest,IServerNetNodeRequest
    {
        public virtual NetNode GetNetNode(string servicename)
        {
            throw new NotImplementedException();
        }
    }
}
