/*
   Copyright 2020 (c) Riccardo Cecchini
   
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using FractalMachineLib.Classes;
using FractalMachineLib.Code.Components;
using FractalMachineLib.Code.Langs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FractalMachineLib.Code
{
    abstract public class Component
    {
        internal Component attached;
        internal string name;
        internal Types type;
        internal Linear _linear;
        internal Component parent;

        /// Compiling purposes
        internal string lastPath;

        public Dictionary<string, Component> components = new Dictionary<string, Component>();
        internal Dictionary<string, string> parameters = new Dictionary<string, string>();

        public Component(Component parent, string Name, Linear Linear) 
        {
            this.parent = parent;
            _linear = Linear;

            if (parent != null && Name != null)
                parent.addComponent(Name, this);

            if (_linear != null)
            {
                _linear.component = this;
                if(!(this is Components.Container)) ReadLinear();
            }
        }

        #region ReadLinear

        public virtual void ReadLinear() { } 

        #endregion

        #region ComponentTypes

        public enum Types
        {
            Container,
            Function,
            Member,
            Operation
        }

        #endregion

        #region Type

        Type _returnType;

        internal Type returnType
        {
            get { return _returnType; }
            set
            {
                _returnType = value;
                TopFile?.UsedType(value);
            }
        }

        #endregion

        #region AddComponents

        internal Component getBaseComponent(string Name, out string toCreate)
        {
            var names = Name.Split('.').ToList();
            toCreate = names.Pull();

            Component baseComp = this;
            if (names.Count > 0)
                baseComp = Solve(String.Join(".", names));

            return baseComp;
        } 

        internal void addComponent(string Name, Component comp)
        {
            string toCreate;
            var baseComp = getBaseComponent(Name, out toCreate);

            baseComp.components[toCreate] = comp;
            comp.name = toCreate;
        }

        internal Component getComponent(string Name)
        {
            string toCreate;
            var baseComp = getBaseComponent(Name, out toCreate);
            Component comp;
            baseComp.components.TryGetValue(Name, out comp);
            return comp;
        }

        #endregion

        #region SubComponents

        public Component SolveNativeFunction(string Name)
        {
            ///
            /// Check native function call
            /// 
            if (Name.StartsWith(Properties.NativeFunctionPrefix))
            {
                // Is C function
                if (Name.StartsWith(Properties.NativeFunctionPrefix + "c_"))
                {
                    Name = Name.Substring(5);
                    var spl = Name.Split('_');
                    var lib = spl[0];
                    Name = spl[1];

                    TopFile.IncludeDefault(lib);

                    //Create dummy component function
                    var fun = new Function(null, null);
                    fun.name = Name;
                    return fun;
                }
            }

            ///
            /// Check internal var reference
            ///
            if (this is Components.Container)
            {
                var cont = (Components.Container)this;
                var ivar = cont.ivarMan.Get(Name);
                if (ivar != null)
                    return new Member(ivar);
            }

            return null;
        }

        public Component Solve(string Name, bool DontPanic = false)
        {
            var parts = Name.Split('.');
            return Solve(parts, DontPanic);
        }

        public virtual Component Solve(string[] Names, bool DontPanic = false, int Level = 0)
        {
            var name = Names[Level];

            Component ground = this, outComp;
            if (Level == 0) 
            {
                // Look for native function
                if (Names.Length == 1)
                {
                    var comp = SolveNativeFunction(name);
                    if (comp != null) return comp;
                }

                // Seek ground
                Component bcomp;
                while (!ground.components.TryGetValue(name, out bcomp)) //check if ground has requested component
                {
                    ground = ground.parent;
                    if (ground == null)
                    {
                        if (!DontPanic) throw new Exception("Error, " + name + " not found");
                        return null;
                    }
                }
            }

            if (ground.components.TryGetValue(name, out outComp))
            {
                Level++;

                if (Level == Names.Length)
                    return outComp;
                else
                    return outComp.Solve(Names, DontPanic, Level);
            }

            if (!DontPanic)
            {
                string path = Names[0];
                for (int i = 1; i < Names.Length - 1; i++) path += "." + Names[i];
                throw new Exception("Error, " + path + " not found");
            }

            return null;
        }

        #endregion

        #region Modifiers

        public virtual bool IsPublic
        {
            get
            {
                // Light modifiers pespective
                return _linear.Attributes.Contains("public");
            }
        }

        public virtual bool CanAccess(Component from)
        {
            if (parent == from)
                return true;

            throw new Exception("todo");
        }

        #endregion

        #region Properties

        public Component Top
        {
            get
            {
                return (parent!=null && parent != this) ? parent.Top : this;
            }
        }

        public virtual Components.File TopFile
        {
            get
            {
                return parent?.TopFile;
            }
        }

        public Components.Container TopContainer
        {
            get
            {
                if (this is Components.Container)
                    return (Components.Container)this;
                else
                    return parent.TopContainer;
            }
        }

        public virtual Project GetProject
        {
            get
            {
                return parent.GetProject;
            }
        }

        #endregion

        #region Methods

        public virtual string GetRealName(Component relativeTo = null)
        {
            return name;

            // to think...
            string topName = null;

            bool hasCommonDescending = relativeTo != null && HasCommonDescending(relativeTo);

            /*
                This is a delicate part. The type should be specified if part of a static class or namespace
                The code below, pratically, is not working for the moment
            */
            if (TopFile != parent && !hasCommonDescending && !(this is DataStructure))
                topName = parent?.GetRealName(relativeTo);

            return (topName != null ? topName + '.' : "") + name;
        }

        public string CalculateRealPath(string Path, Lang Lang = null)
        {
            var ls = Lang?.GetSettings;

            string rp = "";
            var parts = Path.Split('.');

            Component comp = this, lastComp = null;
            foreach(var part in parts)
            {
                comp = comp.Solve(part);

                if (lastComp != null)
                {
                    if (ls != null)
                        rp += ls.VarsDelimiter(lastComp, comp);
                    else
                        rp += ".";
                }
               
                rp += comp.GetRealName();

                lastComp = comp;
            }

            return rp;
        }

        public string GetPath(string Delimiter = ".", Component RelativeTo = null)
        {
            var n = "";
            if (parent != null && parent != TopFile && !HasCommonDescending(RelativeTo)) n = parent.GetPath(Delimiter) + Delimiter;
            return n + name;
        }

        public bool HasCommonDescending(Component comp)
        {
            var p = comp;
            while(p != null)
            {
                if (p == this)
                    return true;

                p = p.parent;
            }

            return false;
        }

        #endregion

        #region Called

        internal bool _called = false;

        public virtual bool Called
        {
            get
            {
                if (!(this is Components.Container))
                    return true;

                if (_called) return true;
                foreach (var comp in components)
                {
                    if (comp.Value.Called) return true;
                }

                return false;
            }
        }

        public Linear Linear
        {
            get
            {
                return _linear;
            }
        }

        #endregion

        #region Writer

        internal bool written = false;
        internal int writeContLength = 0;
        internal List<string> writeCont = new List<string>();
        internal Component writeRedirectTo;

        abstract public string WriteTo(Lang Lang);

        internal virtual void writeReset()
        {
            writeCont.Clear();
            writeContLength = 0;

            if (written) {
                written = false;
                foreach (var i in components)
                    i.Value.writeReset();
            }
        }

        internal virtual int writeNewLine(Linear instr, bool isBase = true)
        {
            if(isBase) writeToCont("\n");
            return parent.writeNewLine(instr, false);
        }

        internal void writeToCont(string str)
        {
            writeCont.Add(str);
            writeContLength += str.Length;
        }

        internal string writeReturn()
        {
            var strBuild = new StringBuilder(writeContLength);
            strBuild.AppendJoin("", writeCont.ToArray());

            if(writeRedirectTo != null)
            {
                writeRedirectTo.writeCont.Add(strBuild.ToString());
                return "";
            }

            return strBuild.ToString();
        }
        
        internal void writeRedirect(Component to)
        {
            writeRedirectTo = to;
        }

        #endregion
    }
}
