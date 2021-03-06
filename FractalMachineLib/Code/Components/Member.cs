using System;
using System.Collections.Generic;
using System.Text;

namespace FractalMachineLib.Code.Components
{
    class Member : Component
    {
        MemberType memberType;

        internal InternalVariablesManager.InternalVariable iVar;
        internal bool typeToBeDefined = false;

        public Member(Component parent, string name, Linear linear):base(parent, name, linear)
        {
            type = Types.Member;
            memberType = MemberType.Normal;

            TopFile.UsedType(returnType);
        }

        public Member(InternalVariablesManager.InternalVariable iVar):base(null, null, null)
        {
            this.iVar = iVar;
            returnType = iVar.type;
        }

        public enum MemberType
        {
            Normal // Variable
            //for the future: Properties
        }

        public override Component Solve(string[] Names, bool DontPanic = false, int Level = 0)
        {
            var name = Names[Level];

            if (returnType.IsDataStructure)
                return returnType.Component.Solve(Names, DontPanic, Level);
            else 
                return base.Solve(Names, DontPanic, Level);
        }

        #region Modifiers

        public override bool IsPublic
        {
            get
            {
                if (iVar != null)
                    return true;

                return base.IsPublic; 
            }
        }

        #endregion

        public override string WriteTo(Lang Lang)
        {
            var ts = Lang.GetTypesSet;

            switch (_linear.Op)
            {
                case "declare":

                    writeToCont(ts.GetTypeCodeName(returnType));
                    writeToCont(" ");
                    writeToCont(_linear.Name);
                    writeToCont(";");

                    break;
            }

            return writeReturn(); 
        }
    }
}
