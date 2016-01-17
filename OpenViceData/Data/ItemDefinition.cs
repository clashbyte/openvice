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
		/// Flags for this entry<para/>
		/// Флаги для этого объекта
		/// </summary>
		public uint Flags { get; set; }

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

	}
}
