using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalMachineLib
{
    public class Conditions
    {
        internal Dictionary<string, dynamic> Dict = new Dictionary<string, dynamic>();
        Conditions Parent;

        public Conditions() { }
        public Conditions(Conditions parent)
        {
            Parent = parent;
        }

        public dynamic this[string index]
        {
            get
            {
                return Dict[index];
            }

            set
            {
                Dict[index] = value;
            }
        }

        public bool IsCompatibleWith(Conditions conds, dynamic dogmas = null)
        {
            return true;
        }

        public void ApplyConditionFrom(Conditions from)
        {
            foreach(var vk in from.Dict)
            {
                var t = vk.Value.GetType();

                if(t == typeof(Status))
                {
                    var v = (Status)t.Value;

                    switch (v)
                    {
                        case Status.Toggle:
                            bool b;
                            //if(!Dict.TryGetValue())

                            break;
                    }

                }
            }
        }

        public enum Status
        {
            Toggle
        }
    }
}
