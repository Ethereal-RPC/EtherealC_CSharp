using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Core.Interface
{
    public interface IBaseIoc
    {
        public void RegisterIoc(string name, object instance);
        public void UnRegisterIoc(string name);
        public bool GetIocObject(string name, out object instance);
    }
}
