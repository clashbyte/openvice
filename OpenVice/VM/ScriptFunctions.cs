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
    /// Opcodes: https://gtamods.com/wiki/List_of_opcodes
    /// </summary>
    public class ScriptFunctions
    {
        public static ScriptModule MainModule;

        static ScriptFunctions()
        {
            MainModule = new ScriptModule("main"); //main.scm
            MainModule.Bind<ScriptFunction>(0000, 0, Nope);
            MainModule.Bind<ScriptFunction>(0001, 1, Wait);
            MainModule.Bind<ScriptFunction>(0002, 1, Goto);
            MainModule.Bind<ScriptFunction>(0004, 2, IntRightAssignment);
            MainModule.Bind<ScriptFunction>(0005, 2, FloatRightAssignment);
            MainModule.Bind<ScriptFunction>(0006, 2, IntLeftAssignment);
            MainModule.Bind<ScriptFunction>(0007, 2, FloatLeftAssignment);
            MainModule.Bind<ScriptFunction>(0008, 2, IntRightAdditionAssignment);
            MainModule.Bind<ScriptFunction>(0x000a, 2, FloatRightAdditionAssignment);
            MainModule.Bind<ScriptFunction>(0x000b, 2, IntLeftAdditionAssignment);
            MainModule.Bind<ScriptFunction>(0x000c, 2, FloatLeftAdditionAssignment);
            MainModule.Bind<ScriptFunction>(0x000d, 2, IntRightSubtractionAssignment);
            MainModule.Bind<ScriptFunction>(0x000e, 2, FloatRightSubtractionAssignment);
            MainModule.Bind<ScriptFunction>(0x000f, 2, FloatLeftSubtractionAssignment);
            MainModule.Bind<ScriptFunction>(0x0010, 2, IntRightMultiplicationAssignment);
            MainModule.Bind<ScriptFunction>(0x0011, 2, FloatRightMultiplicationAssignment);
            MainModule.Bind<ScriptFunction>(0x0012, 2, IntLeftMultiplicationAssignment);
            MainModule.Bind<ScriptFunction>(0x0013, 2, FloatLeftMultiplicationAssignment);
            MainModule.Bind<ScriptFunction>(0x0014, 2, IntRightDivisionAssignment);
            MainModule.Bind<ScriptFunction>(0x0015, 2, FloatRightDivisionAssignment);
            MainModule.Bind<ScriptFunction>(0x0016, 2, IntLeftDivisionAssignment);
            MainModule.Bind<ScriptFunction>(0x0017, 2, FloatLeftDivisionAssignment);
            MainModule.Bind<ScriptFunctionBoolean>(0x0018, 2, IntMoreThanRight);
            MainModule.Bind<ScriptFunctionBoolean>(0x0019, 2, IntMoreThanLeft);
            MainModule.Bind<ScriptFunctionBoolean>(0x001a, 2, IntMoreThanRight2);
            MainModule.Bind<ScriptFunctionBoolean>(0x001b, 2, IntMoreThanLeft2);
            MainModule.Bind<ScriptFunctionBoolean>(0x001c, 2, IntMoreThanRight3);
            MainModule.Bind<ScriptFunctionBoolean>(0x001d, 2, IntMoreThanLeft3);
            MainModule.Bind<ScriptFunctionBoolean>(0x001e, 2, IntMoreThanRight4);
            MainModule.Bind<ScriptFunctionBoolean>(0x001f, 2, IntMoreThanLeft4);
            MainModule.Bind<ScriptFunctionBoolean>(0x0020, 2, FloatMoreThanRight);
            MainModule.Bind<ScriptFunctionBoolean>(0x0021, 2, FloatMoreThanLeft);
            MainModule.Bind<ScriptFunctionBoolean>(0x0022, 2, FloatMoreThanRight2);
            MainModule.Bind<ScriptFunctionBoolean>(0x0023, 2, FloatMoreThanLeft2);
            MainModule.Bind<ScriptFunctionBoolean>(0x0024, 2, FloatMoreThanRight3);
            MainModule.Bind<ScriptFunctionBoolean>(0x0025, 2, FloatMoreThanLeft3);
            MainModule.Bind<ScriptFunctionBoolean>(0x0026, 2, FloatMoreThanRight4);
            MainModule.Bind<ScriptFunctionBoolean>(0x0027, 2, FloatMoreThanLeft4);
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
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntRightAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Integer = Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatRightAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Real = Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntLeftAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer = Args[1].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatLeftAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real = Args[1].Real;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntRightAdditionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Integer += Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatRightAdditionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Real += Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntLeftAdditionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer += Args[1].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatLeftAdditionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real += Args[1].Real;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntRightSubtractionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Integer -= Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatRightSubtractionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Real -= Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntLeftSubtractionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer -= Args[1].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatLeftSubtractionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real -= Args[1].Real;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntRightMultiplicationAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Integer *= Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatRightMultiplicationAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            SecondParam.Real *= Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntLeftMultiplicationAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer *= Args[1].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatLeftMultiplicationAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real *= Args[0].Real;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntRightDivisionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Integer /= Args[0].Integer;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatRightDivisionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter SecondParam = Params[1];

            //I'm assuming the assignment happens right to left here, as that is default for C++.
            SecondParam.Real /= Args[0].Real;
            Params[1] = SecondParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void IntLeftDivisionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Integer /= Args[0].Integer;
            Params[0] = FirstParam;
        }

        /// <summary>
        /// Assigns the value of arg1 to arg2.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void FloatLeftDivisionAssignment(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            SCMOpcodeParameter FirstParam = Params[0];

            FirstParam.Real /= Args[0].Real;
            Params[0] = FirstParam;
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
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IntMoreThanRight(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from left to right.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IntMoreThanLeft(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IntMoreThanRight2(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from left to right.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IntMoreThanLeft2(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IntMoreThanRight3(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IntMoreThanLeft3(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IntMoreThanRight4(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool IntMoreThanLeft4(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool FloatMoreThanRight(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from left to right.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool FloatMoreThanLeft(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool FloatMoreThanRight2(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from left to right.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool FloatMoreThanLeft2(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool FloatMoreThanRight3(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool FloatMoreThanLeft3(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool FloatMoreThanRight4(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[1].Integer > Params[0].Integer;
        }

        /// <summary>
        /// Returns true if arg 1 is more than arg 2. Evaluates from right to left.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        /// <returns></returns>
        public static bool FloatMoreThanLeft4(ref ScriptArguments Args)
        {
            var Params = Args.GetParameters();
            return Params[0].Integer > Params[1].Integer;
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
