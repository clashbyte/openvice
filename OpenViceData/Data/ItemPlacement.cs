using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Data {

	/// <summary>
	/// IDE file parser<para/>
	/// Парсер для IDE-файлов
	/// </summary>
	public class ItemPlacement {

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
		/// Position in 3D space<para/>
		/// Расположение в 3D-мире
		/// </summary>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Scale in 3D space<para/>
		/// Размер в 3D-мире
		/// </summary>
		public Vector3 Scale { get; set; }

		/// <summary>
		/// Angles in 3D space<para/>
		/// Поворот в 3D-мире
		/// </summary>
		public Quaternion Angle { get; set; }

		/// <summary>
		/// Object interior<para/>
		/// Идентификатор интерьера
		/// </summary>
		public Interior InteriorID { get; set; }

		/// <summary>
		/// Hardcoded interior identifiers<para/>
		/// Жёстко зашитые интерьеры
		/// </summary>
		public enum Interior : int {
			World			= 0,
			Hotel			= 1,
			Mansion			= 2,
			Bank			= 3,
			Mall			= 4,
			StripClub		= 5,
			Lawyers			= 6,
			CoffeeShop		= 7,
			ConcertHall		= 8,
			Studio			= 9,
			RifleRange		= 10,
			BikerBar		= 11,
			PoliceStation	= 12,
			Everywhere		= 13,
			DirtRing		= 14,
			BloodRing		= 15,
			HotRing			= 16,
			MalibuClub		= 17,
			PrintWorks		= 18
		}

	}
}
