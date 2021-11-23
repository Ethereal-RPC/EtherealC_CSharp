using System.Collections.Generic;

namespace EtherealC.Core.Model
{
    public class ServerRequestModel
    {
        private string type = "ER-1.0-ServerRequest";
        private string mapping;
        private Dictionary<string,string> @params;
        private string service;

        public string Type { get => type; set => type = value; }
        public string Mapping { get => mapping; set => mapping = value; }
        public Dictionary<string, string> Params { get => @params; set => @params = value; }
        public string Service { get => service; set => service = value; }

        public override string ToString()
        {
            return "ServerRequestModel{" +
                    "type='" + type + '\'' +
                    ", methodId='" + mapping + '\'' +
                    ", params=" + string.Join(",", @params) +
                    ", service='" + service + '\'' +
                    '}';
        }
    }
}
