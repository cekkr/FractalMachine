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
            var trgString = ruleString.NewTrigger("'");
            // works on conditions
            ruleString.Conditions["domain:string"] = Conditions.Status.Toggle;

            var trgStringEscape = ruleString.NewTrigger("\\");
            trgStringEscape.Conditions["domain"] = "string";
            trgStringEscape.OnWinner.Add(delegate (Reader reader)
            {
                reader.AppendToPiece(reader.Eat());
                return true;
            });

            ///
            /// Blocks
            ///
            Rule.Container ruleBlock = (Rule.Container)NewRule(new Rule.Container(this), "block");
            var trgBlock = ruleBlock.NewTriggers("{", "}");

            ///
            /// New Line
            ///
            Rule ruleLine = NewRule("newLine");
            var trgNewLine = ruleLine.NewTrigger("\n");
        }

        public Reader.Piece Interpret(string Str)
        {
            return Reader.Read(Str);
        } 

        public Rule NewRule(string Name = "")
        {
            var rule = new Rule(this);
            rule.Name = Name;
            Rules.Add(rule);
            return rule;
        }

        public Rule NewRule(Rule rule, string Name = "")
        {
            Rules.Add(rule);
            if (!String.IsNullOrEmpty(Name)) rule.Name = Name;
            return rule;
        }

        public class Rule
        {
            public string Name;
            public Interpreter MyInter;
            public Conditions Conditions = new Conditions();            

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

            #region Particulars

            public class Container : Rule
            {
                public Container(Interpreter Inter) : base(Inter) { }

                public Reader.Trigger NewTriggers(string Open, string Close=null)
                {                    
                    if (Close == null)
                        Close = Open;

                    Conditions["$container" + Open + Close] = true;

                    Reader.Trigger trgOp = base.NewTrigger(Open);
                    Reader.Trigger trgCl = trgOp;

                    if (Open != Close)
                    {
                        trgCl = NewTrigger(Close);
                        trgCl.Enabled = false;
                    }

                    int opened = 0;

                    OnWinner.Add(delegate (Reader.Trigger trg)
                    {
                        if(trgOp == trgCl)
                        {
                            if(opened == 0)
                            {
                                opened = 1;
                            }
                            else
                            {
                                opened = 0;
                            }
                        }
                        else
                        {
                            if(trg == trgOp)
                            {
                                opened++;
                                trgCl.Enabled = true;
                            }
                            else
                            {
                                if (--opened == 0)
                                    trgCl.Enabled = false;
                            }
                        }

                        return true;
                    });

                    return trgOp;
                }
            }

            #endregion

        }
    }
}
