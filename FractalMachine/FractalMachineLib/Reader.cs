using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalMachineLib
{
    public class Reader
    {
        public List<Rule> Rules = new List<Rule>();

        public Reader()
        {
            InitBaseRules();
        }

        public void InitBaseRules()
        {
            var separator = NewRule("separator");
        }

        #region Reading

        public string CurStr;
        public int Pos;
        public Conditions CurConditions;
        internal List<Trigger> Triggers = new List<Trigger>();

        public void Read(string Str)
        {
            CurStr = Str;
            CurConditions = new Conditions();

            // Make sure that triggers are well ordered
            Triggers = Triggers.OrderByDescending(o => o.Str.Length).ToList();

            for (Pos=0; Pos< CurStr.Length; Pos++)
            {
                foreach(var trg in Triggers)
                {
                    if (CheckTrigger(trg))
                    {

                    }
                }
            }
        }

        #endregion

        public bool CheckTrigger(Trigger trigger)
        {

            return false;
        }

        public bool CheckString(string Str)
        {
            return false;
        }

        public Rule NewRule(string Name="")
        {
            var rule = new Rule(this);
            rule.Name = Name;
            Rules.Add(rule);
            return rule;
        }

        public class Piece
        {
            public string Content;
            public int Line, Col;
        }

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
        }

        public class Rule
        {
            public string Name;
            public Reader MyReader;

            public Rule(Reader Reader)
            {
                MyReader = Reader;
            }

            public Trigger NewTrigger(string Str = "")
            {
                var trigger = new Trigger(this);
                trigger.Str = Str;
                MyReader.Triggers.Add(trigger);
                return trigger;
            }

        }

        public class Trigger
        {
            public Rule Parent;
            public string Str;

            public Trigger(Rule parent)
            {
                Parent = parent;
            }
        }
    }
}
