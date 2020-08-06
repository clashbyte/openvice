using OpenVice.Controls;
using OpenVice.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenVice.Files;
using OpenVice.Graphics;
using System.IO.Pipes;
using System.Runtime.InteropServices;

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

		static AnimationFile.Animation animation;

		static List<Graphics.Renderers.DebugRenderer.Line> lines;

		static float time = 0;

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
			animation = fg["abseil"];


			man = new Model(new ModelFile(ArchiveManager.Get("HMOCA.dff"), true), true, true);
			manTex = new TextureDictionary(new TextureFile(ArchiveManager.Get("HMOCA.txd"), true), true, true);

			ApplyAnimationFrame(man.Children, animation, 2);

			re = new Graphics.Renderers.SkinnedRenderer();
			lines = new List<Graphics.Renderers.DebugRenderer.Line>();
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

			ApplyAnimationFrame(man.Children, animation, time);
			time += 0.016f;
			time = time % animation.Length;
			

			box.Matrix = t.Matrix;
		}

		/// <summary>
		/// Render debugging stuff<para/>
		/// Отрисовка дебаг-данных
		/// </summary>
		public static void Render() {
			foreach (Graphics.Renderers.DebugRenderer.Line line in lines) {
				rend.Primitives.Remove(line);
			}
			lines.Clear();
			RenderBonesRecursively(man.Children);


			Graphics.Renderer.RenderQueue.Enqueue(re);
			Graphics.Renderer.RenderQueue.Enqueue(rend);
		}


		static void RenderBonesRecursively(Model.Branch[] branches) {
			foreach (Model.Branch b in branches) {
				if (b.Parent!=null) {
					Graphics.Renderers.DebugRenderer.Line line = new Graphics.Renderers.DebugRenderer.Line() {
						Start = b.Matrix.ExtractTranslation() * 10f,
						End = b.Parent.Matrix.ExtractTranslation() * 10f,
						Color = new Vector3(1f, 1f, 0f),
						LineSize = 3f
					};
					lines.Add(line);
					rend.Primitives.Add(line);
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

		static void ApplyAnimationFrame(Model.Branch[] branches, AnimationFile.Animation animation, float time = 0) {
			foreach (Model.Branch b in branches) {
				AnimationFile.Bone boneDef = animation[b.Name];
				if (boneDef != null) {

					
					
					Vector3 transition = Vector3.Zero;
					Quaternion rotation = b.OriginalAngles;
					float scale = b.OriginalScale;
					AnimationFile.Frame frameFrom, frameTo;
					float delta = 0;

					frameFrom = SeekFrameReverse(boneDef, time, 0);
					frameTo = SeekFrame(boneDef, time, 0);
					if (frameFrom != null && frameTo != null) {
						delta = (time - frameFrom.Delay) / (frameTo.Delay - frameFrom.Delay);
						rotation = Quaternion.Slerp(frameFrom.Rotation, frameTo.Rotation, delta);//Vector3.Lerp(frameFrom.Transition, frameTo.Transition, delta);
					}

					frameFrom = SeekFrameReverse(boneDef, time, 1);
					frameTo = SeekFrame(boneDef, time, 1);
					if (frameFrom != null && frameTo != null) {
						delta = (time - frameFrom.Delay) / (frameTo.Delay - frameFrom.Delay);
						transition = Vector3.Lerp(frameFrom.Transition, frameTo.Transition, delta);
						if (b.Name == "Root") {
							//transition.X = 0;
							transition.Z = 0;
						}
					}


					b.Position = b.OriginalPosition + transition;
					b.Angles = rotation;
					b.Scale = scale;

				}
				if (b.Children != null) {
					ApplyAnimationFrame(b.Children, animation, time);
				}
			}
		}

		static AnimationFile.Frame SeekFrame(AnimationFile.Bone bone, float time, int mode = 0) {
			foreach (AnimationFile.Frame frame in bone.Frames) {
				if (frame.Delay > time) {
					bool ok = false;
					switch (mode) {

						case 1:
							ok = frame.HasTransition;
							break;

						case 2:
							ok = frame.HasScale;
							break;


						default:
							ok = true;
							break;
					}
					if (ok) {
						return frame;
					}
				}
			}
			return null;
		}

		static AnimationFile.Frame SeekFrameReverse(AnimationFile.Bone bone, float time, int mode = 0) {
			AnimationFile.Frame[] frames = (AnimationFile.Frame[])bone.Frames.Clone();
			Array.Reverse(frames);
			foreach (AnimationFile.Frame frame in frames) {
				if (frame.Delay <= time) {
					bool ok = false;
					switch (mode) {

						case 1:
							ok = frame.HasTransition;
							break;

						case 2:
							ok = frame.HasScale;
							break;


						default:
							ok = true;
							break;
					}
					if (ok) {
						return frame;
					}
				}
			}
			return null;
		}

	}
}
