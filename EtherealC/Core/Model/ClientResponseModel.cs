namespace EtherealC.Core.Model
{
    public class ClientResponseModel
    {
        private string type = "ER-1.0-ClientResponse";
        private string result = null;
        private Error error = null;
        private string id = null;

        public string Type { get => type; set => type = value; }
        public string Result { get => result; set => result = value; }
        public Error Error { get => error; set => error = value; }
        public string Id { get => id; set => id = value; }

        public override string ToString()
        {
            return "ClientResponseModel{" +
                    "type='" + type + '\'' +
                    ", result='" + result + '\'' +
                    ", error=" + error +
                    ", id='" + id + '\'' +
                    '}';
        }
    }
}
