namespace EtherealC.Model
{
    public class ClientResponseModel
    {
        private string type = "ER-1.0-ClientResponse";
        private object result = null;
        private Error error = null;
        private string id = null;
        private string service = null;
        private string resultType;

        public string Type { get => type; set => type = value; }
        public object Result { get => result; set => result = value; }
        public Error Error { get => error; set => error = value; }
        public string Id { get => id; set => id = value; }
        public string ResultType { get => resultType; set => resultType = value; }
        public string Service { get => service; set => service = value; }

        public override string ToString()
        {

            return "Type:" + Type + "\n"
                + "Id:" + Id + "\n"
                + "Result:" + Result + "\n";
        }
    }
}
