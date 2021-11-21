using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalMachineLib
{
    public class Reader
    {        
        public Reader()
        {
        }

        #region Reading

        public string CurStr;
        public int Pos;
        public Conditions CurConditions;
        internal List<Trigger> Triggers = new List<Trigger>();
        public Piece CurPiece;

        public void Read(string Str)
        {
            CurStr = Str;
            CurConditions = new Conditions();

            List<Piece> Pieces = new List<Piece>();
            CurPiece = new Piece();

            // Make sure that triggers are well ordered
            Triggers = Triggers.OrderByDescending(o => o.Str.Length).ToList();

            for (Pos=0; Pos< CurStr.Length; Pos++)
            {
                var ignoredRules = new List<Interpreter.Rule>();

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

        public class Piece
        {
            public string Content;
            public int Line=0, Col=0;
        }

        public class Trigger
        {
            public Interpreter.Rule Parent;
            public string Str = "";
            public Utils.Callbacks<Reader> Checkers = new Utils.Callbacks<Reader>();

            public Trigger(Interpreter.Rule parent)
            {
                Parent = parent;
            }
        }
    }
}
