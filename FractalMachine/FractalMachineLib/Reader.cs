using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FractalMachineLib.Utils;

namespace FractalMachineLib
{
    public class Reader
    {        
        public Reader()
        {
        }

        internal List<Trigger> Triggers = new List<Trigger>();

        #region Reading

        public char CurCh = '\0';
        public string CurStr;
        public int CurPos;
        public Conditions CurConditions;        
        public Piece CurPiece;

        public void Read(string Str)
        {
            CurStr = Str;
            CurConditions = new Conditions();

            List<Piece> Pieces = new List<Piece>();
            CurPiece = new Piece();

            // Make sure that triggers are well ordered
            Triggers = Triggers.OrderByDescending(o => o.Str.Length).ToList();

            for (CurPos=0; CurPos< CurStr.Length; CurPos++)
            {
                CurCh = Str[CurPos];
                var ignoredRules = new List<Interpreter.Rule>(); //think about it

                bool noTriggersMatch = true;
                foreach(var trg in Triggers)
                {
                    if (trg.Enabled)
                    {
                        // serve a: boh (approfondisci)
                        if (ignoredRules.IndexOf(trg.Parent) < 0)
                        {
                            if (trg.Parent.Conditions.IsCompatibleWith(CurConditions, new { rule = CurPiece.Rule?.Name }))
                            {
                                if (CheckTrigger(trg))
                                {
                                    CurConditions.ApplyConditionFrom(trg.Parent.Conditions);
                                    WinnerTrigger(trg);
                                    noTriggersMatch = false;
                                }
                            }
                            else
                            {
                                ignoredRules.Add(trg.Parent);
                            }
                        }

                        // Special: NoTriggersActivated
                        if (noTriggersMatch)
                        {
                            var winTrg = Triggers.Where(t => t.Special == Special.NoTriggersActivated).FirstOrDefault();
                            if (winTrg != null)
                                WinnerTrigger(winTrg);

                        }
                    }
                }
            }
        }

        void WinnerTrigger(Trigger trg)
        {
            trg.Parent.OnWinner.Call(trg);
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
            public Interpreter.Rule Rule;
            public string Content;
            public int Line=0, Col=0;
        }

        public enum Special
        {
            None,
            NoTriggersActivated
        }

        public class Trigger
        {
            public bool Enabled = true;
            public Interpreter.Rule Parent;
            public string Str = "";
            public Utils.Callbacks<Reader> Checkers = new Utils.Callbacks<Reader>();
            public Special Special = Special.None;
            public Conditions Conditions = new Conditions();

            public Trigger(Interpreter.Rule parent)
            {
                Parent = parent;
            }
        }
    }
}
