using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenVice.Entities;

namespace OpenVice.VM
{
    public delegate void ScriptFunction(ScriptArguments Args);
    public delegate bool ScriptFunctionBoolean(ScriptArguments Args);

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
        public int Arguments;
        /** API name for this function */
        public string Signature;
        /** Human friendly description */
        public string Description;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct SCMOpcodeParameter : IEnumerable<SCMType>
    {
        public SCMOpcodeParameter(SCMType Type)
        {
            type = Type;
            Integer = 0;
            Real = 0;
            Str = "";
            GlobalInteger = 0;
            GlobalReal = 0;
            GlobalPtr = new IntPtr();
        }

        [FieldOffset(0)]
        SCMType type;

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
        float GlobalReal;

        public int IntegerValue()
        {
            switch (type)
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
            switch (type)
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
            return type == SCMType.TLocal || type == SCMType.TGlobal;
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
