using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenVice.Files {

	/// <summary>
	/// Class that reads IFP files<para/>
	/// Класс, читающий IFP-файлы
	/// </summary>
	public class AnimationFile {

		/// <summary>
		/// Animations for this file<para/>
		/// Анимации данного файла
		/// </summary>
		public Animation[] Animations;

		/// <summary>
		/// Read animations from file<para/>
		/// Чтение анимаций из файла
		/// </summary>
		/// <param name="name">File name<para/>Имя файла</param>
		public AnimationFile(string name) {
			if (!File.Exists(name)) {
				throw new FileNotFoundException("[AnimationFile] File not found: " + Path.GetFileName(name), name);
			}
			ReadData(new FileStream(name, FileMode.Open, FileAccess.Read));
		}

		/// <summary>
		/// Read animations from binary data<para/>
		/// Чтение данных анимации из бинарного потока
		/// </summary>
		/// <param name="data">File contents<para/>Содержимое файла</param>
		public AnimationFile(byte[] data) {
			ReadData(new MemoryStream(data));
		}

		
		public Animation this[string name] {
			get {
				if (Animations != null) {
					foreach (Animation anim in Animations) {
						if (anim.Name.ToLower() == name.ToLower()) {
							return anim;
						}
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Read file content from stream<para/>
		/// Чтение файла из потока
		/// </summary>
		/// <param name="stream">Stream<para/>Поток</param>
		void ReadData(Stream stream) {
			// Creating reader
			// Создание ридера
			BinaryReader f = new BinaryReader(stream, Encoding.ASCII);
			Header h;

			// Reading data
			// Чтение данных
			h = ReadHeader(f, HeaderType.AnimationPackage);
			h = ReadHeader(f, HeaderType.Collection);

			// Reading animations
			// Чтение анимаций
			int animCount = f.ReadInt32();
			string nullName = f.ReadOctetString();
			Dev.Console.Log(nullName);

			Animations = new Animation[animCount];
			for (int i = 0; i < animCount; i++) {
				Animations[i] = ReadAnimation(f);
			}

			// Closing stream
			// Закрытие потока
			f.Close();
		}

		/// <summary>
		/// Read single animation from file<para/>
		/// Чтение одной анимации из файла
		/// </summary>
		Animation ReadAnimation(BinaryReader f) {
			Animation a = new Animation();
			Header h;

			// Reading animation name
			// Чтение имени анимации
			h = ReadHeader(f, HeaderType.Name);
			a.Name = f.ReadVCString(h.Size.AlignToOctet());
			System.Diagnostics.Debug.WriteLine(a.Name);

			// Reading objects
			// Чтение объектов
			h = ReadHeader(f, HeaderType.Objects);
			h = ReadHeader(f, HeaderType.Collection);

			int objectCount = f.ReadInt32();
			f.BaseStream.Position += 4;

			a.Bones = new Bone[objectCount];
			for (int i = 0; i < objectCount; i++) {
				a.Bones[i] = ReadBone(f);
				foreach (Frame frame in a.Bones[i].Frames) {
					a.Length = Math.Max(frame.Delay, a.Length);
				}
			}

			return a;
		}

		/// <summary>
		/// Read single bone from animation file<para/>
		/// Чтение одной кости из файла
		/// </summary>
		Bone ReadBone(BinaryReader f) {
			Bone b = new Bone();
			Header h;

			// Reading header
			// Чтение заголовка
			h = ReadHeader(f, HeaderType.Animation);
			h = ReadHeader(f, HeaderType.AnimationData);

			// Reading bone
			// Чтение кости
			b.Name = f.ReadVCString(28);
			b.FrameСount = f.ReadInt32();

			// Skip unknown info
			// Пропуск неизвестной информации
			if (h.Size == 48) {
				f.BaseStream.Position += 4;
			}

			b.LastFrame = f.ReadInt32();
			b.NextSibling = f.ReadInt32();
			b.PrevSibling = f.ReadInt32();

			b.Frames = ReadFrames(f, b.FrameСount);
			return b;
		}

		/// <summary>
		/// Read frames for bone from stream<para/>
		/// Чтение кадров анимации из файла
		/// </summary>
		Frame[] ReadFrames(BinaryReader f, int count) {
			
			// Reading header
			// Чтение заголовка
			Header h = ReadHeader(f);

			// Detecting single frame size
			// Определение размера одного кадра
			bool havePos = false;
			bool haveScale = false;
			if (h.Type == HeaderType.RotationTransitionKey) {
				havePos = true;
			} else if (h.Type == HeaderType.RotationTransitionScaleKey) {
				havePos = true;
				haveScale = true;
			}

			// Reading frames
			// Чтение кадров
			Frame[] frames = new Frame[count];
			for (int i = 0; i < count; i++) {
				Frame fr = new Frame();

				// Rotation
				// Поворот
				fr.Rotation = f.ReadVCQuaternion();

				// Transition
				// Перемещение
				if (havePos) {
					fr.Transition = f.ReadVCVector();
					fr.HasTransition = true;
				}

				// Scale
				// Изменение размера
				if (haveScale) {
					fr.Scale = f.ReadVCVector();
					fr.HasScale = true;
				}

				// Frame time
				// Время кадра
				fr.Delay = f.ReadSingle();
				frames[i] = fr;
			}
			return frames;
		}

		/// <summary>
		/// Read single chunk header<para/>
		/// Чтение заголовка одного чанка
		/// </summary>
		/// <param name="f">BinaryReader</param>
		/// <param name="validate">Throw error if chunk does not match this parameter<para/>Выбрасывается исключение, если тип чанка не совпадает с указанным</param>
		static Header ReadHeader(BinaryReader f, HeaderType validate = HeaderType.Any) {
			Header h = new Header();

			// Reading base sections
			// Чтение базовых секций
			string n = new string(f.ReadChars(4));
			h.Size = f.ReadInt32();

			// Determine chunk type
			// Определение типа чанка
			switch (n.ToUpper()) {
				case "ANPK":
					h.Type = HeaderType.AnimationPackage;
					break;
				case "INFO":
					h.Type = HeaderType.Collection;
					break;
				case "NAME":
					h.Type = HeaderType.Name;
					break;
				case "DGAN":
					h.Type = HeaderType.Objects;
					break;
				case "CPAN":
					h.Type = HeaderType.Animation;
					break;
				case "ANIM":
					h.Type = HeaderType.AnimationData;
					break;
				case "KR00":
					h.Type = HeaderType.RotationKey;
					break;
				case "KRT0":
					h.Type = HeaderType.RotationTransitionKey;
					break;
				case "KRTS":
					h.Type = HeaderType.RotationTransitionScaleKey;
					break;
				default:
					throw new Exception("[AnimationFile] Unknown chunk type: "+n);
			}

			// Validate
			// Проверка
			if (validate != HeaderType.Any) {
				if (h.Type != validate) {
					throw new Exception("[AnimationFile] Unexpected chunk type: " + h.Type);
				}
			}
			return h;
		}

		/// <summary>
		/// Single animation track from file<para/>
		/// Одна анимация из файла
		/// </summary>
		public class Animation {
			/// <summary>
			/// Animation name<para/>
			/// Имя анимации
			/// </summary>
			public string Name;

			/// <summary>
			/// Bones for animation<para/>
			/// Кости анимации
			/// </summary>
			public Bone[] Bones;

			/// <summary>
			/// Animation length
			/// </summary>
			public float Length;

			/// <summary>
			/// Get animation frames by bone name
			/// </summary>
			/// <param name="name">Name</param>
			/// <returns>Bone definition or null</returns>
			public Bone this[string name] {
				get {
					foreach (Bone bone in Bones) {
						if (bone.Name.ToLower() == name.ToLower()) {
							return bone;
						}
					}
					return null;
				}
			}
		}

		/// <summary>
		/// Single animation bone<para/>
		/// Анимируемая кость (объект)
		/// </summary>
		public class Bone {
			/// <summary>
			/// Bone name<para/>
			/// Имя кости
			/// </summary>
			public string Name;

			/// <summary>
			/// Number of frames<para/>
			/// Количество кадров
			/// </summary>
			public int FrameСount;

			/// <summary>
			/// Index of the last frame<para/>
			/// Индекс последнего кадра
			/// </summary>
			public int LastFrame;

			/// <summary>
			/// Next sibling for this bone<para/>
			/// Следующий родственник
			/// </summary>
			public int NextSibling;

			/// <summary>
			/// Previous sibling for this bone<para/>
			/// Предыдущий родственник
			/// </summary>
			public int PrevSibling;

			/// <summary>
			/// Bone frames<para/>
			/// Кадры кости
			/// </summary>
			public Frame[] Frames;
		}

		/// <summary>
		/// Single animation frame<para/>
		/// Один кадр анимации
		/// </summary>
		public class Frame {
			/// <summary>
			/// Frame time<para/>
			/// Время кадра
			/// </summary>
			public float Delay;

			/// <summary>
			/// Flag that keyframe have transition data<para/>
			/// Флаг, что кадр имеет данные передвижения
			/// </summary>
			public bool HasTransition;

			/// <summary>
			/// Flag that keyframe have scaling data<para/>
			/// Флаг, что кадр имеет данные размера
			/// </summary>
			public bool HasScale;

			/// <summary>
			/// Frame rotation data<para/>
			/// Данные о повороте
			/// </summary>
			public Quaternion Rotation;

			/// <summary>
			/// Frame transition data<para/>
			/// Данные о перемещении
			/// </summary>
			public Vector3 Transition;

			/// <summary>
			/// Frame scaling data<para/>
			/// Данные об изменении размера
			/// </summary>
			public Vector3 Scale;

		}

		/// <summary>
		/// Single chunk header<para/>
		/// Один заголовок чанка
		/// </summary>
		struct Header {
			/// <summary>
			/// Type of this chunk<para/>
			/// Тип данного чанка
			/// </summary>
			public HeaderType Type;
			/// <summary>
			/// Chunk size in bytes<para/>
			/// Размер данного чанка
			/// </summary>
			public int Size;
		}

		/// <summary>
		/// Header types for chunks<para/>
		/// Тип заголовка
		/// </summary>
		enum HeaderType {
			Any,
			AnimationPackage,
			Collection,
			Name,
			Objects,
			Animation,
			AnimationData,
			RotationKey,
			RotationTransitionKey,
			RotationTransitionScaleKey
		}
	}
}
