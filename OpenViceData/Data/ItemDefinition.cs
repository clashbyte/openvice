using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Data {

	/// <summary>
	/// Single item definition<para/>
	/// Описание одного предмета
	/// </summary>
	public class ItemDefinition {

		/// <summary>
		/// Unique identifier<para/>
		/// Уникальный идентификатор
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// DFF model name<para/>
		/// Имя DFF модели
		/// </summary>
		public string ModelName { get; set; }

		/// <summary>
		/// TXD dictionary name<para/>
		/// Имя TXD-архива
		/// </summary>
		public string TexDictionary { get; set; }

		/// <summary>
		/// Draw distance for each mesh<para/>
		/// Дистанция отрисовки для каждого меша
		/// </summary>
		public float[] DrawDistance { get; set; }

		/// <summary>
		/// Object is time-based<para/>
		/// Объект зависит от времени
		/// </summary>
		public bool IsTimed { get; set; }

		/// <summary>
		/// Hour when object appears<para/>
		/// Час когда объект появляется
		/// </summary>
		public int TimeOn { get; set; }

		/// <summary>
		/// Hour when object disappears<para/>
		/// Час когда объект пропадает
		/// </summary>
		public int TimeOff { get; set; }

		/// <summary>
		/// Definition flags<para/>
		/// Флаги дефайна
		/// </summary>
		public FlagsContainer Flags { get; set; }

		/// <summary>
		/// Internal class that handles flag operating<para/>
		/// Внутренний класс, который обрабатывает флаги
		/// </summary>
		public class FlagsContainer {
			/// <summary>
			/// Internal flag values<para/>
			/// Внутренние значения флагов
			/// </summary>
			public uint Container { get; private set; }

			/// <summary>
			/// Create new instance from defined flags<para/>
			/// Создание элемента из существующих флагов
			/// </summary>
			/// <param name="f">Defined flags<para/>Заданные флаги</param>
			public FlagsContainer(uint f) {
				Container = f;
			}

			/// <summary>
			/// Indexer for fast flag access<para/>
			/// Индексер для быстрого доступа к флагам
			/// </summary>
			/// <param name="flag">Definition flag<para/>Флаг дефайна</param>
			/// <returns>True if flag is set<para/>True если флаг установлен</returns>
			public bool this[DefinitionFlags flag] {
				get {
					return (Container & (uint)flag) != 0;
				}
			}
		}

		/// <summary>
		/// Flags enum for definition<para/>
		/// Список доступных флагов для дефайна
		/// </summary>
		public enum DefinitionFlags : uint {
			WetObject				= 1,
			FadeDisabled			= 2,
			FullyTransparent		= 4,
			AlphaBlended			= 8,
			FadeEnabled				= 16,
			InteriorOnly			= 32,
			DisableShadow			= 64,
			AlwaysVisible			= 128,
			DisableDrawDistance		= 256,
			BreakableObject			= 512,
			CrackBreakableObject	= 1024
		}
	}
}
