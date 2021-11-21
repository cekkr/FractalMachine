using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalMachineLib
{
    public class Conditions
    {
        Dictionary<string, dynamic> Dict = new Dictionary<string, dynamic>();
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

        public bool IsCompatibleWith(Conditions conds)
        {
            return true;
        }
    }
}
