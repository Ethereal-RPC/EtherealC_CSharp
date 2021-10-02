﻿using Newtonsoft.Json;

namespace EtherealC.Core.Model
{
    public class ServerRequestModel
    {
        private string type= "ER-1.0-ServerRequest";
        private string methodId;
        private object[] @params;
        private string service;

        public string Type { get => type; set => type = value; }
        public string MethodId { get => methodId; set => methodId = value; }
        public object[] Params { get => @params; set => @params = value; }
        public string Service { get => service; set => service = value; }

        public ServerRequestModel(string service,string id,string methodid, object[] @params)
        {
            MethodId = methodid;
            Params = @params;
            Service = service;
        }
        public override string ToString()
        {
            return "ServerRequestModel{" +
                    "type='" + type + '\'' +
                    ", methodId='" + methodId + '\'' +
                    ", params=" + string.Join("参数：", @params) +
                    ", service='" + service + '\'' +
                    '}';
        }
    }
}
