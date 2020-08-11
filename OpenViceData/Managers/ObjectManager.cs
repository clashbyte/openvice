using OpenTK;
using OpenVice.Data;
using OpenVice.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Managers {

	/// <summary>
	/// Manager for all object definitions and placements<para/>
	/// Менеджер всех объявлений объектов и их расстановок
	/// </summary>
	public static class ObjectManager {

		/// <summary>
		/// Definition of all the objects<para/>
		/// Объявление всех объектов
		/// </summary>
		public static ItemDefinition[] Definitions { get; private set; }

		/// <summary>
		/// Placement of all the objects<para/>
		/// Расстановка всех объектов
		/// </summary>
		public static ItemPlacement[] Placements { get; private set; }


		/// <summary>
		/// Parsing IDE files<para/>
		/// Разбор файлов IDE
		/// </summary>
		public static void ReadDefinitions() {
			Definitions = new ItemDefinition[8192];

			int totalEntries = 0;
			foreach (string IDE in FileManager.DefinitionFiles) {
				TextFile f = new TextFile(PathManager.GetAbsolute(IDE), true, true);
				foreach (TextFile.Line p in f.Lines) {
					switch (p.Section.ToLower()) {

						// Common objects
						// Обычные объекты
						case "objs":
							ItemDefinition d = new ItemDefinition() {
								ID = p.Text[0].ToInt(),
								ModelName = p.Text[1].ToLower(),
								TexDictionary = p.Text[2].ToLower(),
								DrawDistance = new float[p.Text[3].ToInt()],
								IsTimed = false,
								Flags = new ItemDefinition.FlagsContainer(p.Text[p.Text.Length - 1].ToUInt())
							};
							for (int i = 0; i < d.DrawDistance.Length; i++) {
								d.DrawDistance[i] = p.Text[4 + i].ToFloat();
							}
							Definitions[d.ID] = d;
							totalEntries++;
							break;

						// Timed objects
						// Временные объекты
						case "tobj":
							d = new ItemDefinition() {
								ID = p.Text[0].ToInt(),
								ModelName = p.Text[1].ToLower(),
								TexDictionary = p.Text[2].ToLower(),
								DrawDistance = new float[p.Text[3].ToInt()],
								IsTimed = true,
								Flags = new ItemDefinition.FlagsContainer(p.Text[p.Text.Length - 3].ToUInt()),
								TimeOn = p.Text[p.Text.Length - 2].ToInt(),
								TimeOff = p.Text[p.Text.Length - 1].ToInt()
							};
							for (int i = 0; i < d.DrawDistance.Length; i++) {
								d.DrawDistance[i] = p.Text[4 + i].ToFloat();
							}
							Definitions[d.ID] = d;
							totalEntries++;
							break;


						default:
							break;
					}

					// Operation is threadable, so sleep for a while
					// Операция потоковая, поэтому отдадим времени основному процессу
					System.Threading.Thread.Sleep(0);
				}
			}
			Dev.Console.Log("[ObjectManager] Parsed " + FileManager.DefinitionFiles.Length + " definition files (" + totalEntries + " entries)");

		}

		/// <summary>
		/// Parsing IPL files<para/>
		/// Разбор файлов IPL
		/// </summary>
		public static void ReadPlacements() {
			List<ItemPlacement> pl = new List<ItemPlacement>();
			foreach (string IPL in FileManager.PlacementFiles) {
				TextFile f = new TextFile(PathManager.GetAbsolute(IPL), true, true);
				foreach (TextFile.Line p in f.Lines) {
					switch (p.Section.ToLower()) {

						// Object installation
						// Расстановка объектов
						case "inst":
                            ItemPlacement g = new ItemPlacement();

                            g = new ItemPlacement()
                            {
                                ID = p.Text[0].ToInt(),
                                ModelName = p.Text[1],
                                InteriorID = ItemPlacement.Interior.World,
                                Position = new Vector3(p.Text[p.Text.Length - 10].ToFloat(), p.Text[p.Text.Length - 8].ToFloat(), p.Text[p.Text.Length - 9].ToFloat()),
                                Scale = new Vector3(p.Text[p.Text.Length - 7].ToFloat(), p.Text[p.Text.Length - 5].ToFloat(), p.Text[p.Text.Length - 6].ToFloat()),
                                Angle = new Quaternion(-p.Text[p.Text.Length - 4].ToFloat(), -p.Text[p.Text.Length - 2].ToFloat(), -p.Text[p.Text.Length - 3].ToFloat(), -p.Text[p.Text.Length - 1].ToFloat()),
                            };
							if (p.Text.Length>12) {
								g.InteriorID = (ItemPlacement.Interior)p.Text[2].ToInt();
							}
							pl.Add(g);
							break;


					}
					// Operation is threadable, so sleep for a while
					// Операция потоковая, поэтому отдадим времени основному процессу
					System.Threading.Thread.Sleep(0);
				}
			}
			Placements = pl.ToArray();
			Dev.Console.Log("[ObjectManager] Parsed " + FileManager.PlacementFiles.Length + " placement files (" + pl.Count + " entries)");
		}


	}
}
