using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using OpenVice.Files;

namespace OpenVice.VM
{
    /// <summary>
    /// VARIOUS TYPES FROM ORIGINAL SOURCE
    /// Различные типы из исходного кода
    /// 
    /// SCMOpcode = ushort
    /// SCMByte = char
    /// </summary>

    public class SCMException : Exception
    {
        public SCMException() : base()
        {

        }

        public virtual string What()
        {
            return "";
        }
    }

    public class IllegalInstruction : SCMException
    {
        public ushort Opcode; //Original type: SCMOpcode
        public uint Offset;
        protected string thread;

        public override string What()
        {
            return string.Format("Illegal Instruction 0x{0:x4}" + 
                " encountered at offset 0x{1:x4}" +  
                " on thread " + thread, Opcode, Offset);
        }
    }

    public class IllegalInstruction<T> : IllegalInstruction
    {
        public IllegalInstruction(ushort opcode, uint offset, string thread)
        {
            Opcode = opcode;
            Offset = offset;
            base.thread = thread;
        }   
    }

    public class UnknownType : SCMException
    {
        protected string thread;
        public char Type; //Original return type: SCMByte, might change to byte.
        public uint Offset;

        public override string What()
        {
            uint type = (uint)Type;

            return string.Format("Unkown data type 0x" + type.ToString("X") + " encountered at offset " +
                "0x " + Offset.ToString("X") + " on thread " + thread);
        }
    }

    public class UnknownType<T> : UnknownType
    {
        public UnknownType(char type, uint offset, string thread)
        {
            Type = type;
            Offset = offset;
            base.thread = thread;
        }
    }

    /// <summary>
    /// The thread that the script code runs on.
    /// </summary>
    public class SCMThread
    {
        public string Name; //17 chars
        public uint BaseAddress;
        public uint ProgramCounter;

        public uint ConditionCount;
        public bool ConditionResult;
        public byte ConditionMask;
        public bool ConditionAND;

        /** Number of MS until the thread should be awoken (-1 = yielded) */
        /** Количество MS до пробуждения потока(-1 = выдано) */
        public int WakeCounter;
        //SCMByte is defined as a char in original source - might have to redefine as byte...
        public List<char> Locals = new List<char>(ScriptMachine.SCM_THREAD_LOCAL_SIZE * (ScriptMachine.SCM_VARIABLE_SIZE));
        public bool IsMission;

        public bool Finished;

        public uint StackDepth;
        // Stores the return-addresses for calls.
        public List<uint> Calls = new List<uint>(ScriptMachine.SCM_STACK_DEPTH);

        public bool DeathOrArrestCheck;
        public bool WastedOrBusted;

        public bool AllowWaitSkip;
    };

    /// <summary>
    ///  * Implements the actual fetch-execute mechanism for the game script virtual
    ///  * machine.
    ///  *
    ///  * The unit of functionality is an "instruction", which performs a particular
    ///  * task such as creating a vehicle, retrieving an object's position or declaring
    ///  * a new garage.
    ///  *
    ///  * The VM executes multiple pseudo-threads that execute in series.Each thread
    ///  * is represented by SCMThread, which contains the program counter, stack
    ///  * information
    ///  * the thread name and some thread-local variable space.At startup, a single
    ///  * thread is created at address 0, which begins execution.From there, the
    ///  * script
    ///  * may create additional threads.
    ///  *
    ///  * Within ScriptMachine, each thread's program counter is used to execute an
    ///  * instruction
    ///  * by consuming the correct number of arguments, allowing the next instruction
    ///  * to be found,
    ///  * and then dispatching a call to the opcode's function.
    ///  
    ///  * Реализует фактический механизм выборки-выполнения для виртуального скрипта игры.
    ///  * машина.
    ///  *
    ///  * Единица функциональности - это «инструкция», которая выполняет конкретную
    ///  * задача, такая как создание автомобиля, получение местоположения объекта или объявление
    ///  * новый гараж.
    ///  *
    ///  * ВМ выполняет несколько псевдопотоков, которые выполняются последовательно.
    ///  * представлен SCMThread, который содержит счетчик программы, стек
    ///  * Информация
    ///  * имя потока и пространство локальных переменных потока. При запуске один
    ///  * по адресу 0 создается поток, который начинает выполнение.
    ///  * скрипт
    ///  * может создавать дополнительные потоки.
    ///  *
    ///  * В ScriptMachine счетчик программы каждого потока используется для выполнения
    ///  * инструкция
    ///  * потребляя правильное количество аргументов, разрешая следующую инструкцию
    ///  * быть найденным,
    ///  * а затем отправка вызова функции кода операции.
    /// </summary>
    public class ScriptMachine
    {
        public const uint SCM_NEGATE_CONDITIONAL_MASK = 0x8000;
        public const byte SCM_CONDITIONAL_MASK_PASSED = 0xFF;
        public const short SCM_THREAD_LOCAL_SIZE = 256;

        /* Maximum size value that can be stored in each memory address.
         * Changing this will break saves.
         */
        public static /*const*/ byte SCM_VARIABLE_SIZE = 4;
        public static /*const*/ byte SCM_STACK_DEPTH = 4;

        private SCMFile file;
        private ScriptModule module;
        private bool debugFlag;
        private List<SCMThread> m_ActiveThreads;
        //SCMByte is defined as a char in original source - might have to redefine as byte...
        private List<char> globalData;

        public ScriptMachine(SCMFile File, ScriptModule Module)
        {

        }

        private void ExecuteThread(SCMThread Thread, int MSPassed)
        {
            ///TODO: Fully implement this.
            /*auto player = state->world->getPlayer();

            if (player)
            {*/
                if (Thread.IsMission && Thread.DeathOrArrestCheck /*&&
                    (player->isWasted() || player->isBusted())*/)
                {
                    Thread.WastedOrBusted = true;
                    Thread.StackDepth = 0;
                    Thread.ProgramCounter = Thread.Calls[(int)Thread.StackDepth];
                }
            //}

            // There is 02a1 opcode that is used only during "Kingdom Come", which
            // basically acts like a wait command, but waiting time can be skipped
            // by pressing 'X'? PS2 button
            if (Thread.AllowWaitSkip /*&& getState()->input[0].pressed(GameInputState::Jump)*/)
            {
                Thread.WakeCounter = 0;
                Thread.AllowWaitSkip = false;
            }
            if (Thread.WakeCounter > 0)
            {
                Thread.WakeCounter = Math.Max(Thread.WakeCounter - MSPassed, 0);
            }
            if (Thread.WakeCounter > 0) return;

            while (Thread.WakeCounter == 0)
            {
                var PC = Thread.ProgramCounter;
                var Opcode = file.Read<ushort>(PC);

                bool IsNegatedConditional = ((Opcode & SCM_NEGATE_CONDITIONAL_MASK) ==
                                             SCM_NEGATE_CONDITIONAL_MASK);
                Opcode = (ushort)(Opcode & ~SCM_NEGATE_CONDITIONAL_MASK);

                ScriptFunctionMeta FoundCode;
                if (!module.FindOpcode(Opcode, out FoundCode))
                {
                    throw new IllegalInstruction<Exception>(Opcode, PC, Thread.Name);
                }

                ScriptFunctionMeta Code = FoundCode;

                PC += sizeof(ushort);

                List<SCMOpcodeParameter> Parameters = new List<SCMOpcodeParameter>();

                bool HasExtraParameters = Code.Arguments < 0;
                var RequiredParams = Math.Abs(Code.Arguments);

                for (int p = 0; p < RequiredParams || HasExtraParameters; ++p)
                {
                    //byte was originally SCMByte ... might have to change to char.
                    var type_r = file.Read<byte>(PC);
                    var type = (SCMType)type_r;

                    if (type_r > 42)
                    {
                        // for implicit strings, we need the byte we just read.
                        type = SCMType.TString;
                    }
                    else
                    {
                        PC += sizeof(byte);
                    }

                    //TODO: Is this correct?
                    //Parameters.push_back(SCMOpcodeParameter{ type, { 0} });
                    Parameters.Add(new SCMOpcodeParameter(type));

                    switch (type)
                    {
                        case SCMType.EndOfArgList:
                            HasExtraParameters = false;
                            break;
                        case SCMType.TInt8:
                            lock (Parameters) //Thread safety
                            {
                                SCMOpcodeParameter Param = Parameters.LastOrDefault();
                                Param.Integer = file.Read<byte>(PC);
                                Parameters[Parameters.Count - 1] = Param;
                            }
                            PC += sizeof(byte);
                            break;
                        case SCMType.TInt16:
                            lock (Parameters) //Thread safety
                            {
                                SCMOpcodeParameter Param = Parameters.LastOrDefault();
                                Param.Integer = file.Read<short>(PC);
                                Parameters[Parameters.Count - 1] = Param;
                            }
                            PC += sizeof(byte) * 2;
                            break;
                        case SCMType.TGlobal:
                            {
                                var v = file.Read<ushort>(PC);

                                lock (Parameters) //Thread safety
                                {
                                    SCMOpcodeParameter Param = Parameters.LastOrDefault();
                                    //TODO: This was originally GlobalData.data() + v...
                                    Param.GlobalPtr = (IntPtr)globalData.ToArray().Length + v * SCM_VARIABLE_SIZE;
                                    Parameters[Parameters.Count - 1] = Param;
                                }

                                if (v >= file.GlobalsSize)
                                {
                                    Debug.WriteLine("ERROR: ScriptMachine.cs: Global out of bounds! " +
                                        v.ToString() + " " +
                                        file.GlobalsSize.ToString());
                                }
                                PC += sizeof(byte) * 2;
                            }
                            break;
                        case SCMType.TLocal:
                            {
                                var v = file.Read<ushort>(PC);

                                lock (Parameters)
                                {
                                    SCMOpcodeParameter Param = Parameters.LastOrDefault();
                                    //Not sure if this is correct, was originally Locals.data() + v...
                                    Param.GlobalPtr = (IntPtr)Thread.Locals.ToArray().Length + v * SCM_VARIABLE_SIZE;
                                    Parameters[Parameters.Count - 1] = Param;
                                }

                                if (v >= SCM_THREAD_LOCAL_SIZE)
                                {
                                    Debug.WriteLine("Scriptmachine.CS: Local out of bounds!");
                                }
                                PC += sizeof(byte) * 2;
                            }
                            break;
                        case SCMType.TInt32:
                            lock (Parameters) //Thread safety
                            {
                                SCMOpcodeParameter Param = Parameters.LastOrDefault();
                                Param.Integer = file.Read<int>(PC);
                                Parameters[Parameters.Count - 1] = Param;
                            }
                            PC += sizeof(byte) * 4;
                            break;
                        case SCMType.TString:
                            lock (Parameters) //Thread safety
                            {
                                SCMOpcodeParameter Param = Parameters.LastOrDefault();
                                char[] Str = Param.Str.ToArray();
                                //Copy a string from the file data to the string.
                                Array.Copy(file.Data.ToArray(), (int)PC, Str, 0, 8);
                                Param.Str = new string(Str);
                                Parameters[Parameters.Count - 1] = Param;
                            }

                            PC += sizeof(byte) * 8;
                            break;
                        case SCMType.TFloat16:
                            lock (Parameters) //Thread safety
                            {
                                SCMOpcodeParameter Param = Parameters.LastOrDefault();
                                Param.Real = file.Read<short>(PC) / 16f;
                                Parameters[Parameters.Count - 1] = Param;
                            }
                            PC += sizeof(byte) * 2;
                            break;
                        default:
                            throw new UnknownType<Exception>((char)type, PC, Thread.Name);
                    };

                    ScriptArguments Sca = new ScriptArguments(Parameters, Thread, this);

#if RW_SCRIPT_DEBUG
                            static auto sDebugThreadName = getenv("OPENRW_DEBUG_THREAD");
                            if (!sDebugThreadName || strncmp(t.name, sDebugThreadName, 8) == 0) {
                                printf("%8s %01x %06x %04x %s", t.name, t.conditionResult,
                                       t.programCounter, opcode, code.signature.c_str());
                                for (auto& a : sca.getParameters()) {
                                    if (a.type == SCMType::TString) {
                                        printf(" %1x:'%s'", a.type, a.string);
                                    } else if (a.type == SCMType::TFloat16) {
                                        printf(" %1x:%f", a.type, a.realValue());
                                    } else {
                                        printf(" %1x:%d", a.type, a.integerValue());
                                    }
                                }
                                printf("\n");
                            }
#endif

                    // After debugging has been completed, update the program counter
                    Thread.ProgramCounter = PC;

                    Code.Function?.Invoke(Sca);

                    if (IsNegatedConditional)
                    {
                        Thread.ConditionResult = !Thread.ConditionResult;
                    }

                    // Handle conditional results for IF statements.
                    if (Thread.ConditionCount > 0 && Opcode != 0x00D6)  /// @todo add conditional
                                                                   /// flag to opcodes
                                                                   /// instead of checking
                                                                   /// for 0x00D6
                    {
                        --Thread.ConditionCount;
                        if (Thread.ConditionAND)
                        {
                            if (Thread.ConditionResult == false)
                            {
                                Thread.ConditionMask = 0;
                            }
                            else
                            {
                                // t.conditionMask is already set to 0xFF by the if and
                                // opcode.
                            }
                        }
                        else
                        {
                            if ((Thread.ConditionMask != 0) || (Thread.ConditionResult != false))
                                Thread.ConditionMask = 1;
                            else
                                Thread.ConditionMask = 0;
                        }

                        Thread.ConditionResult = (Thread.ConditionMask != 0);
                    }

                    SCMOpcodeParameter P = new SCMOpcodeParameter();
                    P.GlobalPtr = (IntPtr)(Thread.Locals.ToArray().Length + 16 * sizeof(char) * 4);
                    P.GlobalInteger += MSPassed;
                    P.GlobalPtr = (IntPtr)(Thread.Locals.ToArray().Length + 17 * sizeof(char) * 4);
                    P.GlobalInteger += MSPassed;

                    if (Thread.WakeCounter == -1)
                        Thread.WakeCounter = 0;
                }
            }
        }
    }
}
