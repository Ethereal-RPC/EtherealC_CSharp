using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EtherealC.Core.Model
{
    public class RPCTypeConfig
    {

        public Dictionary<Type, RPCType> TypesByType { get; set; } = new Dictionary<Type, RPCType>();
        public Dictionary<string, RPCType> TypesByName { get; set; } = new Dictionary<string, RPCType>();

        public RPCTypeConfig()
        {

        }
        public void Add<T>(string typeName)
        {
            Add<T>(typeName, obj => JsonConvert.SerializeObject(obj), obj => JsonConvert.DeserializeObject<T>(obj));
        }
        public void Add<T>(string typeName, RPCType.SerializeDelegage serializDelegage, RPCType.DeserializeDelegage deserializeDelegage)
        {
            RPCType type = new RPCType();
            type.Name = typeName;
            type.Type = typeof(T);
            if (serializDelegage == null) type.Serialize = obj => JsonConvert.SerializeObject(obj);
            else type.Serialize = serializDelegage;
            if (deserializeDelegage == null) type.Deserialize = obj => JsonConvert.DeserializeObject<T>(obj);
            else type.Deserialize = deserializeDelegage;
            Add(type);
        }
        /// <summary>
        /// 注册RPCType
        /// </summary>
        /// <param name="type">RPCType</param>
        public void Add(RPCType type)
        {
            try
            {
                TypesByName.Add(type.Name, type);
                TypesByType.Add(type.Type, type);
            }
            catch (Exception)
            {
                if (TypesByName.ContainsKey(type.Name) || TypesByType.ContainsKey(type.Type)) Console.WriteLine($"注册类型:{type.Type}转{type.Name}发生异常");
            }
        }
    }
}
