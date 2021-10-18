using EtherealC.Request.Attribute;
using EtherealC.Request.WebSocket;
using EtherealC_Test.Model;
using Request = EtherealC.Request.Abstract.Request;

namespace EtherealC_Test.ServiceDemo
{
    public class ServerRequest: WebSocketRequest,IServerRequest
    {
        public virtual bool Register(string username, long id)
        {
            throw new System.NotImplementedException();
        }

        public virtual bool SendSay(long listener_id, string message)
        {
            throw new System.NotImplementedException();
        }

        public virtual int Add(int a, int b)
        {
            throw new System.NotImplementedException();
        }

        public override void Initialization()
        {

        }

        public override void UnInitialization()
        {

        }
    }
}
