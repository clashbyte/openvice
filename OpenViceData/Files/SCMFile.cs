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
        private SCMTarget m_Target;

        private List<string> m_Models = new List<string>();

        private List<uint> m_MissionOffsets = new List<uint>();

        private uint m_MainSize = 0;
        private uint m_MissionLargestSize = 0;

        private uint m_GlobalSectionOffset = 0;
        private uint m_ModelSectionOffset = 0;
        private uint m_MissionSectionOffset = 0;
        private uint m_CodeSectionOffset = 0;

        /// <summary>
        /// Read scripts from file<para/>
        /// Чтение скрипт из файла
        /// </summary>
        /// <param name="name">File name<para/>Имя файла</param>
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
        /// <param name="data">File contents<para/>Содержимое файла</param>
        public SCMFile(byte[] data)
        {
            ReadData(new MemoryStream(data));
        }

        private void ReadData(Stream SCMStream)
        {
            BinaryReader Reader = new BinaryReader(SCMStream);

            uint JumpOpSize = 2u + 1u + 4u;
            uint JumpParamSize = 2u + 1u;

            Reader.BaseStream.Position = JumpOpSize;
            m_Target = (SCMTarget)Reader.ReadByte();

            m_GlobalSectionOffset = JumpOpSize + 1u;

            Reader.BaseStream.Position = JumpParamSize;
            m_ModelSectionOffset = Reader.ReadUInt32() + JumpOpSize + 1u;

            Reader.BaseStream.Position = m_ModelSectionOffset - JumpOpSize - 1u + JumpParamSize;
            m_MissionSectionOffset = Reader.ReadUInt32() + JumpOpSize + 1u;

            Reader.BaseStream.Position = m_MissionSectionOffset - JumpOpSize - 1u + JumpParamSize;
            m_CodeSectionOffset = Reader.ReadUInt32();

            Reader.BaseStream.Position = m_ModelSectionOffset;
            uint ModelCount = Reader.ReadUInt32();

            for(uint i = 0; i < ModelCount; i++)
                m_Models.Add(Reader.ReadVCString(24));

            Reader.BaseStream.Position = m_MissionSectionOffset;
            m_MainSize = Reader.ReadUInt32();
            m_MissionLargestSize = Reader.ReadUInt32();

            uint MissionCount = Reader.ReadUInt32();
            for (int i = 0; i < MissionCount; i++)
                m_MissionOffsets.Add(Reader.ReadUInt32());
        }
    }
}
