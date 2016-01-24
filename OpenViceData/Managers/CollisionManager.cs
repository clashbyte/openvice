using OpenVice.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenVice.Managers {
	
	/// <summary>
	/// Class that contains all the collisions of the world<para/>
	/// Класс, содержащий все меши столкновений мира
	/// </summary>
	public class CollisionManager {

		/// <summary>
		/// List of all IDE-associated collisions<para/>
		/// Список всех IDE-ассоциированных коллизий
		/// </summary>
		public static Dictionary<int, CollisionFile.Group> Collisions;

		/// <summary>
		/// List of all Model-associated collisions<para/>
		/// Список всех модельно-ориентированных коллизий
		/// </summary>
		public static Dictionary<string, CollisionFile.Group> NamedCollisions;

		/// <summary>
		/// Initialize all the meshes<para/>
		/// Инициализация всех моделей
		/// </summary>
		public static void Init() {

			// Initialize collections
			// Создание коллекций
			Collisions = new Dictionary<int, CollisionFile.Group>();
			NamedCollisions = new Dictionary<string, CollisionFile.Group>();

			// Looping through associated files
			// Проход через ассоциированные файлы
			foreach (string file in FileManager.CollisionFiles) {
				string ffile = PathManager.GetAbsolute(file);
				if (File.Exists(ffile)) {
					CollisionFile cf = new CollisionFile(ffile);
					foreach (CollisionFile.Group g in cf.Collisions) {
						bool addToIndexed = false;
						if (g.ID<ObjectManager.Definitions.Length) {
							if (ObjectManager.Definitions[g.ID]!=null) {
								if (ObjectManager.Definitions[g.ID].ModelName == g.Name) {
									addToIndexed = true;
								}
							}
						}

						// Add mesh to indexed list or model-oriented
						// Добавление меша либо в список
						if (addToIndexed) {
							if (!Collisions.ContainsKey(g.ID)) {
								Collisions.Add(g.ID, g);
							}
						}else{
							if (!NamedCollisions.ContainsKey(g.Name)) {
								NamedCollisions.Add(g.Name, g);
							}
						}
					}
				}else{
					Dev.Console.Log("[CollisionManager] Unable to locate collision archive: "+file);
				}
			}
		}
	}
}
