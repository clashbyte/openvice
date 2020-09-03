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
        static ScriptFunctions()
        {
            ScriptModule Module = new ScriptModule("main"); //main.scm
            Module.Bind<ScriptFunction>(0000, 0, Nope);
            Module.Bind<ScriptFunction>(0001, 1, Wait);
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
    }
}
