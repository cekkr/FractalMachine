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

        public Conditions() 
        {
            this["domain"] = "base";
        }

        public Conditions(Conditions parent) : base()
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
            // Check manually the domain
            if (this["domain"] != conds["domain"])
                return false;

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
                            var pr = vk.Key.Split(':');
                            this[pr[0]] = pr[1];

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
