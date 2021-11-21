using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalMachineLib
{
    public class Interpreter
    {
        public Reader Reader = new Reader();
        public List<Rule> Rules = new List<Rule>();

        public Interpreter()
        {
            Init();
        }

        public void Init()
        {
            var separator = NewRule("separator");
        }

        void Interpret(string Str)
        {

        }

        public Rule NewRule(string Name = "")
        {
            var rule = new Rule(this);
            rule.Name = Name;
            Rules.Add(rule);
            return rule;
        }

        public class Rule
        {
            public string Name;
            public Interpreter MyInter;
            public Conditions Conditions;

            public Utils.Callbacks<Reader.Trigger> OnWinner = new Utils.Callbacks<Reader.Trigger>();

            public Rule(Interpreter Inter)
            {
                MyInter = Inter;
            }

            public Reader.Trigger NewTrigger(string Str = "")
            {
                var trigger = new Reader.Trigger(this);
                trigger.Str = Str;
                MyInter.Reader.Triggers.Add(trigger);
                return trigger;
            }

        }
    }
}
