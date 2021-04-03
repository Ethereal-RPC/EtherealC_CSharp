﻿namespace EtherealC.Model
{
    public class ClientResponseModel
    {
        private string jsonRpc = null;
        private object result = null;
        private Error error = null;
        private string id = null;
        private string service = null;
        private string resultType;

        public string JsonRpc { get => jsonRpc; set => jsonRpc = value; }
        public object Result { get => result; set => result = value; }
        public Error Error { get => error; set => error = value; }
        public string Id { get => id; set => id = value; }
        public string ResultType { get => resultType; set => resultType = value; }
        public string Service { get => service; set => service = value; }

        public override string ToString()
        {

            return "Jsonrpc:" + JsonRpc + "\n"
                + "Id:" + Id + "\n"
                + "Result:" + Result + "\n";
        }
    }
}