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
                var ignoredRules = new List<Rule>();

                foreach(var trg in Triggers)
                {
                    if (ignoredRules.IndexOf(trg.Parent) < 0)
                    {
                        if (trg.Parent.Conditions.IsCompatibleWith(CurConditions))
                        {
                            if (CheckTrigger(trg))
                            {
                                trg.Parent.OnWinner.Call(trg);
                            }
                        }
                        else
                        {
                            ignoredRules.Add(trg.Parent);
                        }
                    }
                }
            }
        }

        #endregion

        public bool CheckTrigger(Trigger trg)
        {
            if(trg.Str != "")
            {
                return CheckString(trg.Str);
            }
            else
            {
                return trg.Checkers.Call(this);
            }
        }

        public bool CheckString(string Str)
        {
            // continue here
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

            public bool IsCompatibleWith(Conditions conds)
            {
                return true;
            }
        }

        public class Rule
        {
            public string Name;
            public Reader MyReader;
            public Conditions Conditions;

            public Utils.Callbacks<Trigger> OnWinner = new Utils.Callbacks<Trigger>();

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
            public string Str = "";
            public Utils.Callbacks<Reader> Checkers = new Utils.Callbacks<Reader>();

            public Trigger(Rule parent)
            {
                Parent = parent;
            }
        }
    }
}
