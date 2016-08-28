using OpenVice.Controls;
using OpenVice.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenVice.Files;
using OpenVice.Graphics;

namespace OpenVice.Dev {

	/// <summary>
	/// Some debugging utilities<para/>
	/// Некоторые дебаг-элементы
	/// </summary>
	public static class DebugStuff {

		static Graphics.Renderers.DebugRenderer rend;
		static Graphics.Renderers.DebugRenderer.Box box;

		static Graphics.Model man;
		static Graphics.TextureDictionary manTex;
		static Graphics.Renderers.SkinnedRenderer re;


		/// <summary>
		/// Initialize debug data<para/>
		/// Инициализация отладочной иноформации
		/// </summary>
		public static void Init() {

			//Entities.Camera.Position = new OpenTK.Vector3(-1647.534f, 26.54692f, -667.5128f);

			rend = new Graphics.Renderers.DebugRenderer();
			box = new Graphics.Renderers.DebugRenderer.Box();
			box.LineSize = 2f;
			box.Size = Vector3.One * 0.5f;
			//rend.Primitives.Add(box);

			AnimationFile fg = new AnimationFile(PathManager.GetAbsolute("anim/ped.ifp"));


			man = new Model(new ModelFile(ArchiveManager.Get("cop.dff"), true), true, true);
			manTex = new TextureDictionary(new TextureFile(ArchiveManager.Get("cop.txd"), true), true, true);
			


			re = new Graphics.Renderers.SkinnedRenderer();
			RenderBonesRecursively(man.Children);
		}

		/// <summary>
		/// Update debugging stuff<para/>
		/// Обновление дебаг-данных
		/// </summary>
		public static void Update() {
			if (Controls.Input.KeyPress(Key.KeypadPlus)) {
				CityManager.Interior = (Data.ItemPlacement.Interior)(((int)CityManager.Interior + 1) % 19);
			}
			if (Controls.Input.KeyPress(Key.Keypad0)) {
				CityManager.Interior = Data.ItemPlacement.Interior.World;
			}

			if (Controls.Input.KeyPress(Key.Q)) {
				PhysicsManager.SetBox(Entities.Camera.Position, Entities.Camera.TransformDirection(Vector3.UnitZ * 50f));
			}

			Graphics.Transform t = new Graphics.Transform() {
				Scale = Vector3.One,
				Position = PhysicsManager.pos,
				Angles = PhysicsManager.rot
			};

			box.Matrix = t.Matrix;
		}

		/// <summary>
		/// Render debugging stuff<para/>
		/// Отрисовка дебаг-данных
		/// </summary>
		public static void Render() {
			Graphics.Renderer.RenderQueue.Enqueue(re);
			Graphics.Renderer.RenderQueue.Enqueue(rend);
		}


		static void RenderBonesRecursively(Model.Branch[] branches) {
			foreach (Model.Branch b in branches) {
				if (b.Parent!=null) {
					rend.Primitives.Add(new Graphics.Renderers.DebugRenderer.Line() { 
						Start = b.Matrix.ExtractTranslation() * 10f,
						End = b.Parent.Matrix.ExtractTranslation() * 10f,
						Color = new Vector3(1f, 1f, 0f),
						LineSize = 3f
					});
				}
				if (b.SubMeshes.Length > 0) {
					re.SubMesh = b.SubMeshes[0];
					re.SubmeshMatrix = b.Matrix;
					re.BaseMatrix = Matrix4.CreateScale(10f);
					re.Textures = manTex;
				}
				if (b.Children!=null) {
					RenderBonesRecursively(b.Children);
				}
			}
		}

	}
}
