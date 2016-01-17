using System;

namespace OpenVice.Dev {

	/// <summary>
	/// Class that handles software DXT decoding on the fly<para/>
	/// Класс, который раскодирует DXT-сжатие на лету
	/// </summary>
	public static class DXTDecoder {

		/// <summary>
		/// DXT compression type<para/>
		/// Тип сжатия DXT
		/// </summary>
		public enum CompressionType : int
		{
			DXT1,
			DXT3,
			DXT5
		}

		/// <summary>
		/// Decode single DXT block<para/>
		/// Разбор одного DXT-блока
		/// </summary>
		/// <param name="rgba">Decoded RGBA-data array<para/>Массив расшифрованных RGBA-данных</param>
		/// <param name="block">Single DXT-data block<para/>Один блок DXT-данных</param>
		/// <param name="type">Decoding type<para/>Режим расшифровки</param>
		static void DecodeBlock(ref byte[] rgba, byte[] block, CompressionType type) {
			// Get the block locations
			// Получение расположения блоков
			byte[] colorBlock = new byte[8];
			byte[] alphaBlock = block;
			if (type == CompressionType.DXT3 || type == CompressionType.DXT5)
				Array.Copy(block, 8, colorBlock, 0, 8);
			else
				Array.Copy(block, 0, colorBlock, 0, 8);

			// Decompress color
			// Расшифровка цвета
			DecodeColor(ref rgba, colorBlock, type == CompressionType.DXT1);

			// Decompress alpha separately if necessary
			// Отдельно расшифровка альфаканала если требуется
			if (type == CompressionType.DXT3) {
				DecodeAlphaDxt3(ref rgba, alphaBlock);
			} else if (type == CompressionType.DXT5) {
				DecodeAlphaDxt5(ref rgba, alphaBlock);
			}
		}

		/// <summary>
		/// Decompress DXT3 alpha data<para/>
		/// Расшифровка DXT3-альфаканала
		/// </summary>
		/// <param name="rgba">Decoded RGBA-data array<para/>Массив расшифрованных RGBA-данных</param>
		/// <param name="block">Single DXT-data block<para/>Один блок DXT-данных</param>
		static void DecodeAlphaDxt3(ref byte[] rgba, byte[] block) {
			byte[] bytes = block;

			// Unpack the alpha values pairwise
			// Попарно расшифровываем значения альфаканала
			for (int i = 0; i < 8; i++)
			{
				// Quantise down to 4 bits
				// Сжимаем до 4 бит
				byte quant = bytes[i];

				byte lo = (byte)(quant & 0x0F);
				byte hi = (byte)(quant & 0xF0);

				// Convert back up to bytes
				// Конвертируем обратно в байты
				rgba[8 * i + 3] = (byte)(lo | (lo << 4));
				rgba[8 * i + 7] = (byte)(hi | (hi >> 4));
			}
		}

		/// <summary>
		/// Decompress DXT5 alpha data<para/>
		/// Расшифровка DXT5-альфаканала
		/// </summary>
		/// <param name="rgba">Decoded RGBA-data array<para/>Массив расшифрованных RGBA-данных</param>
		/// <param name="block">Single DXT-data block<para/>Один блок DXT-данных</param>
		static void DecodeAlphaDxt5(ref byte[] rgba, byte[] block) {
			// Get the two alpha values
			// Получение двух значений альфаканала
			byte[] bytes = block;
			int alpha0 = bytes[0];
			int alpha1 = bytes[1];

			// Compare the values to build the codebook
			// Сравнение значений для построения словаря
			byte[] codes = new byte[8];
			codes[0] = (byte)alpha0;
			codes[1] = (byte)alpha1;
			if (alpha0 <= alpha1) {
				// Use 5-Alpha Codebook
				// Использовать 5 значений словаря
				for (int i = 1; i < 5; i++) {
					codes[1 + i] = (byte)(((5 - i) * alpha0 + i * alpha1) / 5);
				}
				codes[6] = 0;
				codes[7] = 255;
			} else {
				// Use 7-Alpha Codebook
				// Использовать 7 значений словаря
				for (int i = 1; i < 7; i++) {
					codes[i + 1] = (byte)(((7 - i) * alpha0 + i * alpha1) / 7);
				}
			}

			// Decode indices
			// Расшифровка индексов
			byte[] indices = new byte[16];
			byte[] blockSrc = bytes;
			int blockSrc_pos = 2;
			byte[] dest = indices;
			int indices_pos = 0;
			for (int i = 0; i < 2; i++) {
				// Grab 3 bytes
				// Получение 3х байт
				int value = 0;
				for (int j = 0; j < 3; j++) {
					int _byte = blockSrc[blockSrc_pos++];
					value |= (_byte << 8 * j);
				}

				// Unpack 8 3-bit values from it
				// Расшифровываем из них 8 трёхбитных значений
				for (int j = 0; j < 8; j++) {
					int index = (value >> 3 * j) & 0x07;
					dest[indices_pos++] = (byte)index;
				}
			}

			// Write out the indexed codebook values
			// Запись индексированнных словарных значений
			for (int i = 0; i < 16; i++) {
				rgba[4 * i + 3] = codes[indices[i]];
			}
		}

		/// <summary>
		/// Decode single DXT color block<para/>
		/// Расшифровка одного цветового DXT-блока
		/// </summary>
		/// <param name="rgba">Decoded RGBA-data array<para/>Массив расшифрованных RGBA-данных</param>
		/// <param name="block">Single DXT-data block<para/>Один блок DXT-данных</param>
		/// <param name="isDxt1">Flag that we decoding DXT1<para/>Флаг, что расшифровывается DXT1</param>
		static void DecodeColor(ref byte[] rgba, byte[] block, bool isDxt1) {
			byte[] bytes = block;

			// Unpack Endpoints
			// Расшифровка основных цветов
			byte[] codes = new byte[16];
			int a = Unpack565(bytes, 0, ref codes, 0);
			int b = Unpack565(bytes, 2, ref codes, 4);

			// Generate Midpoints
			// Генерация промежуточных цветов
			for (int i = 0; i < 3; i++) {
				int c = codes[i];
				int d = codes[4 + i];

				if (isDxt1 && a <= b){
					codes[8 + i] = (byte)((c + d) / 2);
					codes[12 + i] = 0;
				} else {
					codes[8 + i] = (byte)((2 * c + d) / 3);
					codes[12 + i] = (byte)((c + 2 * d) / 3);
				}
			}

			// Fill in alpha for intermediate values
			// Заполнение альфаканала
			codes[8 + 3] = 255;
			codes[12 + 3] = (isDxt1 && a <= b) ? (byte)0 : (byte)255;

			// Unpack the indices
			// Расшифровка индексов
			byte[] indices = new byte[16];
			for (int i = 0; i < 4; i++) {
				byte packed = bytes[4 + i];
				indices[0 + i * 4] = (byte)(packed & 0x3);
				indices[1 + i * 4] = (byte)((packed >> 2) & 0x3);
				indices[2 + i * 4] = (byte)((packed >> 4) & 0x3);
				indices[3 + i * 4] = (byte)((packed >> 6) & 0x3);
			}

			// Store out the colors
			// Запись цветовых значений
			for (int i = 0; i < 16; i++) {
				byte offset = (byte)(4 * indices[i]);
				for (int j = 0; j < 4; j++) {
					rgba[4 * i + j] = codes[offset + j];
				}
			}
		}

		/// <summary>
		/// Decode 5650-type color to 8888-color<para/>
		/// Расшифровка цвета 5650 в 8888
		/// </summary>
		/// <param name="packed">Packed DXT data<para/>DXT-данные</param>
		/// <param name="packed_offset">Offset in DXT data<para/>Отступ от начала DXT-данных</param>
		/// <param name="color">Output color data<para/>Выходные RGBA-данные</param>
		/// <param name="color_offset">Offset in color data<para/>Отступ от начала RGBA-данных</param>
		/// <returns>32-bit color integer<para/>32-битное цветовое значение</returns>
		static int Unpack565(byte[] packed, int packed_offset, ref byte[] color, int color_offset) {
			// Build packed value
			// Сборка двухбайтового значения
			int value = (int)packed[0 + packed_offset] | ((int)packed[1 + packed_offset] << 8);

			// Get components in the stored range
			// Получение цветовых значений
			byte red = (byte)((value >> 11) & 0x1F);
			byte green = (byte)((value >> 5) & 0x3F);
			byte blue = (byte)(value & 0x1F);

			// Scale up to 8 Bit
			// Конвертация в 8 бит на канал
			color[0 + color_offset] = (byte)((red << 3) | (red >> 2));
			color[1 + color_offset] = (byte)((green << 2) | (green >> 4));
			color[2 + color_offset] = (byte)((blue << 3) | (blue >> 2));
			color[3 + color_offset] = 255;

			return value;
		}

		/// <summary>
		/// Decode DXT image data<para/>
		/// Расшифровка DXT-сжатия
		/// </summary>
		/// <param name="width">Image width<para/>Ширина изображения</param>
		/// <param name="height">Image height<para/>Высота изображения</param>
		/// <param name="data">DXT-data array<para/>Массив DXT-данных</param>
		/// <param name="type">DXT type<para/>Тип DXT-сжатия</param>
		/// <returns>RGBA-values array<para/>Массив RGBA-данных</returns>
		public static byte[] Decode(int width, int height, byte[] data, CompressionType type) {
			// Initialize output
			// Инициализация готовых данных
			byte[] rgba = new byte[width * height * 4];

			// Initialise the block input
			// Инициализация входных данных
			byte[] sourceBlock = data;
			int sourceBlockPos = 0;
			int bytesPerBlock = (type == CompressionType.DXT1) ? 8 : 16;

			// Loop over blocks
			// Проходимся по блокам
			for (int y = 0; y < height; y += 4) {
				for (int x = 0; x < width; x += 4) {

					// Decompress the block
					// Расшифровка блока
					byte[] targetRGBA = new byte[4 * 16];
					byte[] sourceBlockBuffer = new byte[bytesPerBlock];
					Array.Copy(sourceBlock, sourceBlockPos, sourceBlockBuffer, 0, bytesPerBlock);
					DecodeBlock(ref targetRGBA, sourceBlockBuffer, type);

					// Write the decompressed pixels to the correct image locations
					// Запись расшифрованных пикселей
					int targetRGBApos = 0;
					for (int py = 0; py < 4; py++) {
						for (int px = 0; px < 4; px++) {
							int sx = x + px;
							int sy = y + py;
							if (sx < width && sy < height)  {
								Array.Copy(targetRGBA, targetRGBApos, rgba, 4 * (width * sy + sx), 4);
							}
							targetRGBApos += 4;
						}
					}
					sourceBlockPos += bytesPerBlock;
				}
			}

			// Sending back decoded data
			// Отправка расшифрованных данных
			return rgba;
		}
	}
}
