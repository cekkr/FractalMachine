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
            ///
            /// Separator
            ///
            var separator = NewRule("separator");
            separator.NewTrigger(" ");

            ///
            /// Word
            ///
            var word = NewRule("word");
            var wordDynTrgs = word.NewTrigger();
            //wordDynTrgs.Conditions["$rule"] = "!word"; //concept
            wordDynTrgs.Checkers.Add(delegate (Reader reader)
            {
                if (reader.CurPiece.Rule == word)
                    return false;

                var ch = reader.CurCh;

                if (ch == '$')
                    return true;

                return Chars.IsLecter(ch);
            });

            wordDynTrgs.Checkers.Add(delegate (Reader reader)
            {
                if (reader.CurPiece.Rule != word)
                    return false;

                var ch = reader.CurCh;
                return Chars.IsLecter(ch) || Chars.IsNumber(ch);
            });

            ///
            /// String
            ///
            // proof of concept
            var ruleString = NewRule("string");
            var trgString = ruleString.NewTrigger();
            trgString.Str = "'";
            // works on conditions
            ruleString.Conditions["!string"] = Conditions.Status.Toggle;
        }

        void Interpret(string Str)
        {
            Reader.Read(Str);
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

            public Reader.Trigger NewTrigger(Reader.Special cliche)
            {
                var trg = NewTrigger();
                trg.Special = cliche;
                return trg;
            }

        }
    }
}
