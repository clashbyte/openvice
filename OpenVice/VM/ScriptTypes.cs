//////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// This program is free software: you can redistribute it and/or modify it under the terms of the 
/// GNU General Public License as published by the Free Software Foundation, either version 3 of the License, 
/// or (at your option) any later version.
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the 
/// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
/// for more details.
/// You should have received a copy of the GNU General Public License along with this program. If not, see
/// http://www.gnu.org/licenses/.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenVice.Entities;

namespace OpenVice.VM
{
    public delegate void ScriptFunction(ref ScriptArguments Args);
    public delegate bool ScriptFunctionBoolean(ref ScriptArguments Args);

    public class ScriptObjectType<T>
    {
        private int id = 0;
        private T Object = default(T);

        public ScriptObjectType(int Var, GameObject Object)
        {
            id = Var;
            object Obj = Object;
            this.Object = (T)Obj;
        }

        public ScriptObjectType(int Var, T Object)
        {
            id = Var;
            this.Object = Object;
        }

        public T CopyFrom(T Object)
        {
            Debug.Assert(id != 0, "ScriptObjectType has pointer to null memory location");
            //C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            //ORIGINAL LINE: *m_id = static_cast<ScriptInt>(object->getScriptObjectID());
            //m_ID.CopyFrom((int)Object.GetScriptObjectID());
            this.Object = Object;
            return Object;
        }

        public T Dereference()
        {
            Debug.Assert(Object != null, "Dereferencing ScriptObjectType with null instance");
            return Object;
        }

        public T get()
        {
            return Object;
        }

        public static implicit operator T(ScriptObjectType<T> ImpliedObject)
        {
            return ImpliedObject.Object;
        }
    }

    /// <summary>
    /// Enum of opcode argument types
    /// </summary>
    public enum SCMType
    {
        EndOfArgList = 0x00,
        TInt32 = 0x01,
        TGlobal = 0x02,
        TLocal = 0x03,
        TInt8 = 0x04,
        TInt16 = 0x05,
        TFloat16 = 0x06,
        TString = 0x09,
    };

    struct SCMTypeInfo
    {
        byte size;
    };

    public struct ScriptFunctionMeta
    {
        public ScriptFunction Function;
        public ScriptFunctionBoolean BooleanFunction;
        public int Arguments;
        /** API name for this function */
        public string Signature;
        /** Human friendly description */
        public string Description;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct SCMOpcodeParameter
    {
        public SCMOpcodeParameter(SCMType type)
        {
            Type = type;
            Integer = 0;
            Real = 0;
            Str = "";
            GlobalInteger = 0;
            GlobalReal = 0;
            GlobalPtr = new IntPtr();
        }

        [FieldOffset(0)]
        public SCMType Type;

        [FieldOffset(1)]
        public int Integer;
        [FieldOffset(2)]
        public float Real;
        [FieldOffset(3)]
        public string Str;
        [FieldOffset(4)]
        public IntPtr GlobalPtr;
        [FieldOffset(5)]
        public int GlobalInteger;
        [FieldOffset(6)]
        public float GlobalReal;

        public int IntegerValue()
        {
            switch (Type)
            {
                case SCMType.TGlobal:
                case SCMType.TLocal:
                    return GlobalInteger;
                case SCMType.TInt8:
                case SCMType.TInt16:
                case SCMType.TInt32:
                    return Integer;
                default:
                    //TODO: Replace with error system.
                    //RW_ERROR("Unhandled type");
                    return 0;
            }
        }

        public float RealValue()
        {
            switch (Type)
            {
                case SCMType.TGlobal:
                case SCMType.TLocal:
                    return GlobalReal;
                case SCMType.TFloat16:
                    return Real;
                default:
                    //TODO: Replace with error system.
                    //RW_ERROR("Unhandled type");
                    return 0;
            }
        }

        public bool IsLvalue()
        {
            return Type == SCMType.TLocal || Type == SCMType.TGlobal;
        }

        public int HandleValue()
        {
            return GlobalInteger;
        }
    }

    /// <summary>
    /// A class for passing arguments to script functions.
    /// </summary>
    public class ScriptArguments
    {
        private List<SCMOpcodeParameter> parameters = new List<SCMOpcodeParameter>();
        private SCMThread thread;
        private ScriptMachine machine;

        public ScriptArguments(List<SCMOpcodeParameter> P, SCMThread T, ScriptMachine M)
        {
            parameters = P;
            thread = T;
            machine = M;
        }

        /// <summary>
        /// Gets the SCMOpcodeParameter of this ScriptArguments instance.
        /// </summary>
        /// <returns>An SCMOpcodeParameter instance.</returns>
        public List<SCMOpcodeParameter> GetParameters()
        {
            return parameters;
        }

        /// <summary>
        /// Gets the SCMThread of this ScriptArguments instance.
        /// </summary>
        /// <returns>An SCMThread instance.</returns>
        public SCMThread GetThread()
        {
            return thread;
        }

        public ScriptMachine GetVM()
        {
            return machine;
        }

        //TODO: Implement this...
        // Helper method to get the current state
        //GameState* getState() const;
        //GameWorld* getWorld() const;

        public SCMOpcodeParameter this[uint Arg]
        {
            get
            {
                return parameters[(int)Arg];
            }
            set
            {
                parameters[(int)Arg] = value;
            }
        }

        public virtual int GetModel(uint arg) { return 0; }

        public virtual GameObject GetObject<T>(uint arg)
        {
            return null;
        }

        public GameObject GetPlayerCharacter<T>(uint Player)
        {
            return null;
        }

        /// <summary>
        /// Returns the int parameter at the specified index.
        /// </summary>
        /// <param name="Arg">The index of where to retrieve the param.</param>
        /// <returns>An int.</returns>
        public int GetIntParameter(uint Arg)
        {
            return GetParameters()[(int)Arg].IntegerValue();
        }

        /// <summary>
        /// Returns the float parameter at the specified index.
        /// </summary>
        /// <param name="Arg">The index of where to retrieve the param.</param>
        /// <returns>A float.</returns>
        public float GetFloatParameter(uint Arg)
        {
            return GetParameters()[(int)Arg].RealValue();
        }

        /// <summary>
        /// Returns the string parameter at the specified index.
        /// </summary>
        /// <param name="Arg">The index of where to retrieve the param.</param>
        /// <returns>A string.</returns>
        public string GetStringParameter(uint Arg)
        {
            return GetParameters()[(int)Arg].Str;
        }

        public virtual T GetParameterRef<T>(uint Arg)
        {
            return default(T);
        }

        /// <summary>
        /// Returns a handle for the object of type T at the argument index.
        /// </summary>
        /// <typeparam name="T">An object of type T.</typeparam>
        /// <param name="Arg">The index of the argument.</param>
        /// <returns>A ScriptObjectType.</returns>
        public virtual ScriptObjectType<T> GetScriptObject<T>(uint Arg)
        {
            return null;
        }
    }
}
