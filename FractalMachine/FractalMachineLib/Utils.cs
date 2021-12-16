using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalMachineLib
{
    public class Utils
    {
        public class Callbacks<T>
        {
            public delegate bool Delegate(T caller);
            public List<Delegate> List = new List<Delegate>();

            public void Add(Delegate del)
            {
                List.Add(del);
            }

            public bool Call(T Caller)
            {
                foreach(var d in List)
                {
                    if (d.Invoke(Caller))
                        return true;
                }

                return false;
            }

            public bool CallIfNotEmpty(T Caller)
            {
                if (List.Count == 0)
                    return true;

                return Call(Caller);
            }
        }

        public class JObject : DynamicObject
        {

        }
    }
}
