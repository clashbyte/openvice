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
using System.Diagnostics;

namespace OpenVice.VM
{
    /// <summary>
    /// Implements the functions accessible by the scripts.
    /// All the functions are named after definitions found on gtamods.com
    /// Opcodes: https://gtamods.com/wiki/SCM_language_III/VC_definitions
    /// </summary>
    public class ScriptFunctions
    {
        public static ScriptModule MainModule;

        static ScriptFunctions()
        {
            MainModule = new ScriptModule("main"); //main.scm
            MainModule.Bind<ScriptFunction>(0x0000, 0, Nope);
            MainModule.Bind<ScriptFunction>(0x0001, 1, Wait);
            MainModule.Bind<ScriptFunction>(0x0002, 1, Goto);
            MainModule.Bind<ScriptFunction>(0x0004, 2, SetVarInt);
            MainModule.Bind<ScriptFunction>(0x0005, 2, SetVarFloat);
            MainModule.Bind<ScriptFunction>(0x0006, 2, SetLVarInt);
            MainModule.Bind<ScriptFunction>(0x0007, 2, SetLVarFloat);
            MainModule.Bind<ScriptFunction>(0x0008, 2, AddValToIntVar);
            MainModule.Bind<ScriptFunction>(0x000a, 2, AddValToIntLVar);
            MainModule.Bind<ScriptFunction>(0x000b, 2, AddValToFloatLVar);
            MainModule.Bind<ScriptFunction>(0x000c, 2, SubValFromIntVar);
            MainModule.Bind<ScriptFunction>(0x000d, 2, SubValFromFloatVar);
            MainModule.Bind<ScriptFunction>(0x000e, 2, SubValFromIntLVar);
            MainModule.Bind<ScriptFunction>(0x000f, 2, SubValFromFloatLVar);
            MainModule.Bind<ScriptFunction>(0x0010, 2, MultIntVarByVal);
            MainModule.Bind<ScriptFunction>(0x0011, 2, MultFloatLVarByVal);
            MainModule.Bind<ScriptFunction>(0x0012, 2, MultIntLVarByVal);
            MainModule.Bind<ScriptFunction>(0x0013, 2, MultFloatLVarByVal);
            MainModule.Bind<ScriptFunction>(0x0014, 2, DivIntVarByVal);
            MainModule.Bind<ScriptFunction>(0x0015, 2, DivFloatVarByVal);
            MainModule.Bind<ScriptFunction>(0x0016, 2, DivIntLVarByVal);
            MainModule.Bind<ScriptFunction>(0x0017, 2, DivFloatLVarByVal);
            MainModule.Bind<ScriptFunctionBoolean>(0x0018, 2, IsIntVarGreaterThanNumber);
            MainModule.Bind<ScriptFunctionBoolean>(0x0019, 2, IsIntLVarGreaterThanNumber);
            MainModule.Bind<ScriptFunctionBoolean>(0x001a, 2, IsNumberGreaterThanIntVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x001b, 2, IsNumberGreaterThanIntLVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x001c, 2, IsIntVarGreaterThanIntVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x001d, 2, IsIntLVarGreaterThanIntLVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x001e, 2, IsIntVarGreaterThanIntLVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x001f, 2, IsIntLVarGreaterThanIntVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x0020, 2, IsFloatVarGreaterThanNumber);
            MainModule.Bind<ScriptFunctionBoolean>(0x0021, 2, IsFloatLVarGreaterThanNumber);
            MainModule.Bind<ScriptFunctionBoolean>(0x0022, 2, IsNumberGreaterThanFloatVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x0023, 2, IsNumberGreaterThanFloatLVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x0024, 2, IsFloatVarGreaterThanFloatVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x0025, 2, IsFloatLVarGreaterThanFloatLVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x0026, 2, IsFloatVarGreaterThanFloatLVar);
            MainModule.Bind<ScriptFunctionBoolean>(0x0027, 2, IsFloatLVarGreaterThanFloatVar);
        }

        /// <summary>
        /// Nope = no operation.
        /// Opcode: 0000
        /// </summary>
        /// <param name="Args">An instance of ScriptArguments.</param>
        public static void Nope(ref ScriptArguments Args)
        {
            //DO NOTHING! HURRAY!
        }

        /// <summary>
        /// Waits for a specified number of millisecs.
        /// Opcode: 0001
        /// </summary>
        /// <param name="Args">An instance of ScriptArguments.</param>
        public static void Wait(ref ScriptArguments Args)
        {
            int Time = Args.GetIntParameter(0);
            Debug.Assert(Time >= 0, "negative wait time is not supported");
            var Thread = Args.GetThread();
            // Scripts use wait 0 to yield
            Thread.WakeCounter = Time > 0 ? Time : -1;
        }

        /// <summary>
        /// Goes to a label (Arg1).
        /// </summary>
        /// <param name="Args">An int that specifies the address to go to.</param>
        public static void Goto(ref ScriptArguments Args)
        {
            var Thread = Args.GetThread();
            int Arg1 = Args.GetIntParameter(0);
            Thread.ProgramCounter = (uint)(Arg1 < 0 ? Thread.BaseAddress - Arg1 : Arg1);
        }

        public static void ShakeCamera(ref ScriptArguments Args)
        {
            //Args: Time
            //TODO: Shake camera!
        }

        /// <summary>
        /// Sets the value of a global variable to a specified integer.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void SetVarInt(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Integer = Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Sets the value of a global variable to a specified float.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void SetVarFloat(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Real = Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Sets the value of a local variable to a specified integer.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void SetLVarInt(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer = Args[1].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Sets the value of a local variable to a specified float.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void SetLVarFloat(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real = Args[1].Real;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Increases the value stored in the global variable by the specified integer value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void AddValToIntVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Integer += Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Increases the value stored in the global variable by the specified float value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void AddValToFloatVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Real += Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Increases the value stored in the local variable by the specified integer value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void AddValToIntLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer += Args[1].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Increases the value stored in the local variable by the specified float value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void AddValToFloatLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real += Args[1].Real;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Subtracts the specified integer from the value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void SubValFromIntVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Integer -= Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Subtracts the specified float from the value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void SubValFromFloatVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Real -= Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Subtracts the specified int from the value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void SubValFromIntLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer -= Args[1].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Subtracts the specified float from the value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void SubValFromFloatLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real -= Args[1].Real;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Multiplies the value stored in the global variable by the specified integer value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void MultIntVarByVal(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Integer *= Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Multiplies the value stored in the global variable by the specified float value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void MultFloatVarByVal(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Real *= Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Multiplies the value stored in the local variable by the specified integer value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void MultIntLVarByVal(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer *= Args[1].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Multiplies the value stored in the local variable by the specified float value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void MultFloatLVarByVal(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real *= Args[0].Real;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Divides the value stored in the global variable by the specified integer value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void DivIntVarByVal(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Integer /= Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Divides the value stored in the global variable by the specified float value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void DivFloatVarByVal(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Real /= Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Divides the value stored in the local variable by the specified integer value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void DivIntLVarByVal(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer /= Args[0].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Divides the value stored in the local variable by the specified float value.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void DivFloatLVarByVal(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real /= Args[0].Real;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Checks if the integer value stored in the global variable is greater than the integer number.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntVarGreaterThanNumber(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the integer value stored in the local variable is greater than the integer number.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntLVarGreaterThanNumber(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Checks if the integer number is greater than the integer value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns>True if greater than, false otherwise.</returns>
        public static bool IsNumberGreaterThanIntVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the integer number is greater than
        /// the integer value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns>True if greater than, false otherwise.</returns>
        public static bool IsNumberGreaterThanIntLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        //  Checks if the floating-point number is greater than the value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns>True if greater than, false otherwise.</returns>
        public static bool IsNumberGreaterThanFloatVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        //  Checks if the floating-point number is greater than the value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns>True if greater than, false otherwise.</returns>
        public static bool IsNumberGreaterThanFloatLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Checks if the floating-point value stored in the global variable is 
        /// greater than the floating-point value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsFloatVarGreaterThanFloatVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the integer value stored in the local variable is greater or equal to the integer number.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntLVarGreaterOrEqualToNumber(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the floating-point value stored in the local variable is 
        /// greater than the floating-point value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsFloatLVarGreaterThanFloatLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the floating-point value stored in the global variable is 
        /// greater than the floating-point value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsFloatVarGreaterThanFloatLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the floating-point value stored in the local variable is 
        /// greater than the floating-point value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsFloatLVarGreaterThanFloatVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the integer value stored in the global variable is greater or equal to the integer number.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntVarGreaterOrEqualToNumber(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer >= Params[0].Integer;
        }

        /// <summary>
        /// Checks if the integer value stored in the global variable is 
        /// greater than the integer value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntVarGreaterThanIntVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the integer value stored in the local variable is 
        /// greater than the integer value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntLVarGreaterThanIntLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Checks if the integer value stored in the global variable is 
        /// greater than the integer value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntVarGreaterThanIntLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the integer value stored in the local variable is 
        /// greater than the integer value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntLVarGreaterThanIntVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Checks if the value stored in the global variable is greater than the floating-point number.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsFloatVarGreaterThanNumber(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the value stored in the local variable is greater than the floating-point number.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsFloatLVarGreaterThanNumber(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Checks if the integer number is greater or equal to the integer value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsNumberGreaterOrEqualToIntVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer >= Params[0].Integer;
        }

        /// <summary>
        /// Checks if the integer number is greater or equal to the integer value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsNumberGreaterOrEqualToIntLVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer >= Params[1].Integer;
        }

        /// <summary>
        /// Checks if the integer value stored in the global variable is 
        /// greater or equal to the integer value stored in the global variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntVarGreaterOrEqualToIntVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Checks if the integer value stored in the local variable is 
        /// greater or equal to the integer value stored in the local variable.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IsIntLVarGreaterOrEqualToIntVar(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Ends a thread on the ScriptMachine.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void EndThread(ref ScriptArguments Args)
        {
            SCMThread Thread = Args.GetThread();
            Thread.WakeCounter = -1;
            Thread.Finished = true;
        }

        /// <summary>
        /// Starts a new thread on the ScriptMachine.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void StartThread(ref ScriptArguments Args)
        {
            Args.GetVM().StartThread((uint)Args.GetIntParameter(0), false);
            var Threads = Args.GetVM().GetThreads();
            SCMThread Thread = Threads[Threads.Count - 1];

            var Locals = Thread.Locals.ToArray();

            // Copy arguments to locals
            for (var i = 1u; i < Args.GetParameters().Count; ++i)
            {
                if (Args[i].Type == SCMType.EndOfArgList)
    		        break;

                /**reinterpret_cast<ScriptInt*>(Thread.Locals.ToArray() + 
                    sizeof(int) * (i - 1)) = args[i].integerValue();*/
                byte[] SrcInt = BitConverter.GetBytes(Args[i].IntegerValue());
                Array.Copy(SrcInt, 0, Locals, sizeof(int) * (i - 1), SrcInt.Length - 1);
                Thread.Locals.AddRange(Locals);
            }
        }
    }
}
