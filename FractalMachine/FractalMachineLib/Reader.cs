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
        public int CurCol = 0, CurRow = 0;
        public Conditions CurConditions;        
        public Piece CurPiece;
        public Trigger CurTrigger;

        public Piece Read(string Str)
        {
            CurStr = Str;
            CurConditions = new Conditions();

            var MainPiece = CurPiece = new Piece(0, 0);

            // Make sure that triggers are well ordered
            Triggers = Triggers.OrderByDescending(o => o.Str.Length).ToList();

            for (CurPos=0; CurPos< CurStr.Length;)
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
                            // trg.Parent.Conditions.IsCompatibleWith(CurConditions, new { rule = CurPiece.Rule?.Name })
                            if (trg.IsCompatibleWith(CurConditions))
                            {
                                if (CheckTrigger(trg))
                                {
                                    CurConditions.ApplyConditionFrom(trg.Parent.Conditions);
                                    WinnerTrigger(trg);
                                    noTriggersMatch = false;
                                    goto endOfTheLine;
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
                            {
                                WinnerTrigger(winTrg);
                                goto endOfTheLine;
                            }
                        }
                    }
                }

                endOfTheLine:

                // Check piece condition
                if(CurTrigger != CurPiece.Trigger)
                {
                    CurPiece = CurPiece.Add(CurRow, CurCol);
                    CurPiece.Trigger = CurTrigger;
                }

                AppendToPiece(CurCh);

                if (CurCh == '\n')
                {
                    CurCol = 0;
                    CurRow++;
                }
                else
                    CurRow++;
                
            }

            return MainPiece;
        }

        public void AppendToPiece(char Str)
        {
            AppendToPiece(Str.ToString());
        }

        public void AppendToPiece(string Str)
        {
            CurPiece.Content += Str;
            CurPos += Str.Length;
        }

        #region Movements

        public string Eat(int Length = 1)
        {
            string Buf = "";
            int Until = CurPos + Length;
            for (; CurPos < Until; CurPos++)
                Buf += CurStr[CurPos];

            return Buf;
        }

        #endregion

        void WinnerTrigger(Trigger trg)
        {
            if (trg.OnWinner.Call(this) && trg.Parent.OnWinner.Call(trg))
            {
                CurTrigger = trg;
                CurConditions.ApplyConditionFrom(trg.Conditions);
                CurConditions.ApplyConditionFrom(trg.Parent.Conditions);
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
            public Piece Parent;
            public List<Piece> Pieces = new List<Piece>();
            public Interpreter.Rule Rule;
            public string Content;
            public int Row=0, Col=0;

            public Trigger Trigger;

            public Piece(int Row, int Col)
            {
                this.Row = Row;
                this.Col = Col;
            }

            public Piece Add(int Row, int Col)
            {
                // temporary solution
                if (Parent != null)
                    return Parent.Add(Row, Col);

                var p = new Piece(Row, Col);
                p.Parent = this;
                Pieces.Add(p);
                return p;
            }
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
            public Utils.Callbacks<Reader> OnWinner = new Utils.Callbacks<Reader>();
            public Utils.Callbacks<Reader> Checkers = new Utils.Callbacks<Reader>();
            public Special Special = Special.None;
            public Conditions Conditions = new Conditions();

            public Trigger(Interpreter.Rule parent)
            {
                Parent = parent;
            }

            public bool IsCompatibleWith(Conditions From)
            {
                return Conditions.IsCompatibleWith(From) && Parent.Conditions.IsCompatibleWith(From);
            }
        }
    }
}
