using FractalMachineLib.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace FractalMachineLib.Code.Components
{
    public class Operation : Component
    {
        internal bool disabled = false;
        internal InternalVariablesManager.InternalVariable returnVar;

        public Operation(Container parent, Linear linear) : base(parent, null, linear)
        {
            type = Types.Operation;
        }

        public Container Parent
        {
            get
            {
                return (Container)parent;
            }
        }

        override public string WriteTo(Lang Lang)
        {
            if (disabled)
                return "";

            ///
            /// Special treatments
            ///

            // Temporary
            if(_linear.Op == "import")
            {
                TopFile.Include(Lang, _linear.component.attached);
                return "";
            }

            ///
            /// The others
            ///

            var ts = Lang.GetTypesSet;

            ///
            /// Handle return - ie string var = ...
            /// 
            if (!String.IsNullOrEmpty(_linear.Return)) // is it has a sense?
            {
                ///
                /// Get return type
                /// 
                var var = Parent.ivarMan.Get(_linear.Return);
                if (var != null) // Is InternalVariable
                {
                    if (var.IsUsed(_linear))
                    {
                        var.setRealVar(Lang);
                        if (!String.IsNullOrEmpty(var.realVarType))
                        {
                            writeToCont(var.realVarType);
                            writeToCont(" ");
                        }
                        writeToCont(var.realVarName);
                        writeToCont("=");
                    }
                }
                else // is normal variable
                {
                    //todo: handle getter and setter
                    var varPath = Parent.CalculateRealPath(_linear.Return, Lang);
                    writeToCont(varPath);
                    writeToCont("=");
                }
            }

            ///
            /// Operation analyzing
            ///
            if(_linear.HasOperator)
            {
                if (_linear.Type == "=")
                {
                    writeAttributeVariable(Lang, _linear.Name, returnType);
                }
                else
                {
                    string t1 = null, t2 = _linear.Attributes[0];

                    if (_linear.Type != "!")
                    {
                        t1 = t2;
                        t2 = _linear.Attributes[1];
                    }

                    if (t1 != null)
                        writeAttributeVariable(Lang, t1, returnType);

                    writeToCont(_linear.Type);
                    writeAttributeVariable(Lang, t2, returnType);
                }
            }
            else if (_linear.IsCall)
            {
                //var toCall = Parent.Solve(_linear.Name);

                var name = Parent.CalculateRealPath(_linear.Name, Lang);
                writeToCont(name);

                // Write parameters
                writeToCont("(");
                var ac = _linear.Attributes.Count;
                for (var a=0; a<ac; a++)
                {
                    var attr = _linear.Attributes[a];

                    writeAttributeVariable(Lang, attr);

                    if (a < ac-1)
                        writeToCont(",");
                }

                writeToCont(")");

            }
            else if (_linear.IsCast)
            {
                throw new Exception("todo");
            }

            writeToCont(";");

            return writeReturn();
        }

        void checkAttributeTypeAccessibility(AttributeType attrType)
        {
            if (attrType.Type == AttributeType.Types.Name)
            {
                // Check modifier
                var v = Solve(attrType.AbsValue);
                if (!v.IsPublic && !v.CanAccess(Parent))
                    throw new Exception("Variable is inaccessible");
            }
        }

        void writeAttributeVariable(Lang Lang, string attr, Type requestedType = null)
        {
            var ts = Lang.GetTypesSet;
            var attrType = _linear.Lang.GetTypesSet.GetAttributeType(attr);
            checkAttributeTypeAccessibility(attrType);

            if (attrType.Type == AttributeType.Types.Name)
            {
                checkAttributeTypeAccessibility(attrType);
                if (attrType.AbsValue.IsInternalVariable())
                {
                    var iv = Parent.ivarMan.Get(attrType.AbsValue);
                    writeToCont(iv.realVarName);
                }
                else
                {
                    var varPath = Parent.CalculateRealPath(attrType.AbsValue, Lang);
                    writeToCont(varPath); //todo: handle complex var tree (ie Namespace.Var)
                }
            }
            else
            {
                writeToCont(ts.SolveAttributeType(attrType, requestedType));
            }
        }

        #region Override

        public override Component Solve(string[] Names, bool DontPanic = false, int Level = 0)
        {
            return parent.Solve(Names, DontPanic, Level);
        }

        #endregion
    }
}
  