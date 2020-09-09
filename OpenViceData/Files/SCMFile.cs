using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVice.Files
{
    enum SCMTarget { NoTarget = 0, GTAIII = 0xC6, GTAVC = 0x6D, GTASA = 0x73 };

    /// <summary>
    /// Class that reads script files.
    /// </summary>
    public class SCMFile
    {
        private SCMTarget target;

        private BinaryReader reader;

        /// <summary>
        /// Gets the data from this file.
        /// </summary>
        public MemoryStream Data { get; } = new MemoryStream();

        private List<string> models = new List<string>();

        private List<uint> missionOffsets = new List<uint>();

        private uint mainSize = 0;
        private uint missionLargestSize = 0;

        private uint globalSectionOffset = 0;
        private uint modelSectionOffset = 0;
        private uint missionSectionOffset = 0;
        private uint codeSectionOffset = 0;

        /// <summary>
        /// Gets the size of the globals section of this file.
        /// </summary>
        public uint GlobalsSize { get { return modelSectionOffset - globalSectionOffset; } }

        /// <summary>
        /// The offset of the globals section of this file.
        /// </summary>
        public uint GlobalSection { get { return globalSectionOffset; } }

        /// <summary>
        /// Read scripts from file<para/>
        /// Чтение скрипт из файла
        /// </summary>
        /// <param name="name">File name/Имя файла</param>
        public SCMFile(string name)
        {
            if (!File.Exists(name))
            {
                throw new FileNotFoundException("[SCMFile] File not found: " + Path.GetFileName(name), name);
            }

            ReadData(new FileStream(name, FileMode.Open, FileAccess.Read));
        }

        /// <summary>
        /// Read scripts from binary data<para/>
        /// Чтение данных скрипт из бинарного потока
        /// </summary>
        /// <param name="Data">File contents<para/>Содержимое файла</param>
        public SCMFile(byte[] Data)
        {
            ReadData(new MemoryStream(Data));
        }

        private void ReadData(Stream SCMStream)
        {
            reader = new BinaryReader(SCMStream);
            reader.BaseStream.CopyTo(Data);

            uint JumpOpSize = 2u + 1u + 4u;
            uint JumpParamSize = 2u + 1u;

            reader.BaseStream.Position = JumpOpSize;
            target = (SCMTarget)reader.ReadByte();

            globalSectionOffset = JumpOpSize + 1u;

            reader.BaseStream.Position = JumpParamSize;
            modelSectionOffset = reader.ReadUInt32() + JumpOpSize + 1u;

            reader.BaseStream.Position = modelSectionOffset - JumpOpSize - 1u + JumpParamSize;
            missionSectionOffset = reader.ReadUInt32() + JumpOpSize + 1u;

            reader.BaseStream.Position = missionSectionOffset - JumpOpSize - 1u + JumpParamSize;
            codeSectionOffset = reader.ReadUInt32();

            reader.BaseStream.Position = modelSectionOffset;
            uint ModelCount = reader.ReadUInt32();

            for(uint i = 0; i < ModelCount; i++)
                models.Add(reader.ReadVCString(24));

            reader.BaseStream.Position = missionSectionOffset;
            mainSize = reader.ReadUInt32();
            missionLargestSize = reader.ReadUInt32();

            uint MissionCount = reader.ReadUInt32();
            for (int i = 0; i < MissionCount; i++)
                missionOffsets.Add(reader.ReadUInt32());

            reader.Close();
        }

        /// <summary>
        /// Reads a value from this SCMFile instance at the program counter's position.
        /// </summary>
        /// <typeparam name="T">The type to read. Supported types: int, short, uint, ushort, byte, char.</typeparam>
        /// <param name="ProgramCounter">The current position of the Program Counter.</param>
        /// <returns>The value read, or null if the type wasn't supported.</returns>
        public T Read<T>(uint ProgramCounter)
        {
            object ReturnVal = null;

            //TODO: Could this be a different search origin???
            reader.BaseStream.Seek(ProgramCounter, SeekOrigin.Begin);

            if (typeof(T) == typeof(uint))
            {
                ReturnVal = reader.ReadUInt32();
                return (T)ReturnVal;
            }
            if (typeof(T) == typeof(ushort))
            {
                ReturnVal = reader.ReadUInt16();
                return (T)ReturnVal;
            }
            if (typeof(T) == typeof(int))
            {
                ReturnVal = reader.ReadInt32();
                return (T)ReturnVal;
            }
            if (typeof(T) == typeof(short))
            {
                ReturnVal = reader.ReadInt16();
                return (T)ReturnVal;
            }
            if (typeof(T) == typeof(byte))
            {
                ReturnVal = reader.ReadByte();
                return (T)ReturnVal;
            }
            if (typeof(T) == typeof(char))
            {
                ReturnVal = reader.ReadChar();
                return (T)ReturnVal;
            }

            throw new Exception("Tried to read from SCM with unsupported type!");
        }
    }
}
