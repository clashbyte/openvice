using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenVice.Files {

	/// <summary>
	/// Stream for ADF radio files<para/>
	/// Поток для расшифровки ADF-файлов радио
	/// </summary>
	public class AdfStream : Stream {

		/// <summary>
		/// XOR operation key<para/>
		/// Ключ для XOR
		/// </summary>
		const byte XORKey = 0x22;

		/// <summary>
		/// Internal stream<para/>
		/// Внутренний поток
		/// </summary>
		Stream baseStream;

		/// <summary>
		/// Base constructor for ADF stream<para/>
		/// Конструктор для ADF-потока
		/// </summary>
		/// <param name="stream">Existing stream<para/>Открытый поток</param>
		public AdfStream(Stream stream)
		{
			baseStream = stream;
		}

		/// <summary>
		/// <see cref="Stream.CanRead"/>
		/// </summary>
		public override bool CanRead
		{
			get { return baseStream.CanRead; }
		}

		/// <summary>
		/// <see cref="Stream.CanSeek"/>
		/// </summary>
		public override bool CanSeek
		{
			get { return baseStream.CanSeek; }
		}

		/// <summary>
		/// <see cref="Stream.CanWrite"/>
		/// </summary>
		public override bool CanWrite
		{
			get { return false; }
		}

		/// <summary>
		/// <see cref="Stream.Flush()"/>
		/// </summary>
		public override void Flush()
		{
			baseStream.Flush();
		}

		/// <summary>
		/// <see cref="Stream.Length"/>
		/// </summary>
		public override long Length
		{
			get { return baseStream.Length; }
		}

		/// <summary>
		/// <see cref="Stream.Position"/>
		/// </summary>
		public override long Position
		{
			get
			{
				return baseStream.Position;
			}
			set
			{
				Seek(value, SeekOrigin.Begin);
			}
		}

		/// <summary>
		/// <see cref="Stream.Read()"/>
		/// </summary>
		public override int Read(byte[] buffer, int offset, int count)
		{
			byte[] data = new byte[count];
			baseStream.Read(data, 0, count);
			for (int i = 0; i < count; i++) {
				buffer[i + offset] = (byte)(data[i] ^ XORKey);
			}
			return count;
		}

		/// <summary>
		/// <see cref="Stream.ReadByte()"/>
		/// </summary>
		public override int ReadByte() {
			return (byte)(baseStream.ReadByte() ^ XORKey);
		}

		/// <summary>
		/// <see cref="Stream.Seek()"/>
		/// </summary>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return baseStream.Seek(offset, origin);
		}

		/// <summary>
		/// <see cref="Stream.SetLength()"/>
		/// </summary>
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// <see cref="Stream.Write()"/>
		/// </summary>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// <see cref="Stream.Close()"/>
		/// </summary>
		public override void Close()
		{
			baseStream.Close();
		}

	}
}
