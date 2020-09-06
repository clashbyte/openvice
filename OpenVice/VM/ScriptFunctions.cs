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
            MainModule.Bind<ScriptFunction>(0x004f, 2, StartThread);
        }

        /// <summary>
        /// Nope = no operation.
        /// Opcode: 0000
        /// </summary>
        /// <param name="Args">An instance of ScriptArguments.</param>
        public static void Nope(ScriptArguments Args)
        {
            //DO NOTHING! HURRAY!
        }

        /// <summary>
        /// Waits for a specified number of millisecs.
        /// Opcode: 0001
        /// </summary>
        /// <param name="Args">An instance of ScriptArguments.</param>
        public static void Wait(ScriptArguments Args)
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
        public static void Goto(ScriptArguments Args)
        {
            var Thread = Args.GetThread();
            int Arg1 = Args.GetIntParameter(0);
            Thread.ProgramCounter = (uint)(Arg1 < 0 ? Thread.BaseAddress - Arg1 : Arg1);
        }

        public static void ShakeCamera(ScriptArguments Args)
        {
            //Args: Time
            //TODO: Shake camera!
        }

        /// <summary>
        /// Ends a thread on the ScriptMachine.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void EndThread(ScriptArguments Args)
        {
            SCMThread Thread = Args.GetThread();
            Thread.WakeCounter = -1;
            Thread.Finished = true;
        }

        /// <summary>
        /// Starts a new thread on the ScriptMachine.
        /// </summary>
        /// <param name="Args">A ScriptArguments instance.</param>
        public static void StartThread(ScriptArguments Args)
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
