using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenVice.Files;
using OpenVice.Managers;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using OpenTK;

namespace OpenVice.Physics {

	/// <summary>
	/// Collider for world objects<para/>
	/// Коллайдер для мировых объектов
	/// </summary>
	public class StaticCollider {

		/// <summary>
		/// Flag that collider is enabled in simulation<para/>
		/// Флаг что коллайдер включен в симуляцию
		/// </summary>
		public bool IsEnabled { get; private set; }

		/// <summary>
		/// COLL file contents for this collider<para/>
		/// Данные столкновений для коллайдера
		/// </summary>
		public CollisionFile.Group CollisionData { get; private set; }

		/// <summary>
		/// List of bodies for this collider<para/>
		/// Список физтел для данного коллайдера
		/// </summary>
		CollisionEntry[] subColliders;

		/// <summary>
		/// Create new StaticCollider from existing collision data<para/>
		/// Создание нового коллайдера из существующих данных
		/// </summary>
		/// <param name="colmesh">Collision data<para/>Данные о коллизиях</param>
		public StaticCollider(CollisionFile.Group colmesh, Vector3 position, Quaternion angles, Vector3 scale) {

			// Create base transformation matrix
			// Создание базовой матрицы трансформации
			Matrix4 mat =
				Matrix4.CreateScale(scale) *
				Matrix4.CreateFromQuaternion(angles) *
				Matrix4.CreateTranslation(position);

			// Create bodies
			// Создание тел
			List<CollisionEntry> col = new List<CollisionEntry>();

			// Spheres
			// Сферы
			if (colmesh.Spheres!=null) {
				foreach (CollisionFile.Sphere s in colmesh.Spheres) {
					// Transforming positions to world coordinates
					// Трансформация расположения в мировые координаты
					Vector3 pos = Vector3.TransformPosition(s.Center, mat);
					float radius = Vector3.TransformVector(Vector3.UnitX * s.Radius, mat).Length;
					
					// Create primitive
					// Создание примитива
					EntityShape shape = new SphereShape(radius);
					col.Add(new CollisionEntry() {
						Type = PrimitiveType.Sphere,
						Position = pos,
						Rotation = Quaternion.Identity,
						Shape = shape,
						Body = new Entity(shape)
					});
				}
			}

			// Cubes
			// Кубы
			if (colmesh.Boxes!=null) {
				foreach (CollisionFile.Box b in colmesh.Boxes) {
					// Transforming positions to world coordinates
					// Трансформация расположения в мировые координаты
					Vector3 pos = Vector3.TransformPosition(
						new Vector3(
							(b.Min.X+b.Max.X)/2f,
							(b.Min.Y+b.Max.Y)/2f,
							(b.Min.Z+b.Max.Z)/2f
						)
					, mat);
					float factor = Vector3.TransformVector(Vector3.UnitX, mat).Length;

					// Create primitive
					// Создание примитива
					EntityShape shape = new BoxShape(
						(float)Math.Abs(b.Max.X-b.Min.X) * factor,
						(float)Math.Abs(b.Max.Y-b.Min.Y) * factor,
						(float)Math.Abs(b.Max.Z-b.Min.Z) * factor
					);
					col.Add(new CollisionEntry() {
						Type = PrimitiveType.Box,
						Position = pos,
						Rotation = angles,
						Shape = shape,
						Body = new Entity(shape)
					});
				}
			}

			// Trimeshes
			// Тримеши
			if (colmesh.Meshes!=null) {
				
				// Creating vertices array
				// Создание массива вершин
				BEPUutilities.Vector3[] verts = new BEPUutilities.Vector3[colmesh.Vertices.Length];
				for (int i = 0; i < colmesh.Vertices.Length; i++)
				{
					verts[i] = new BEPUutilities.Vector3(
						colmesh.Vertices[i].X, 
						colmesh.Vertices[i].Y, 
						colmesh.Vertices[i].Z
					);
				}

				foreach (CollisionFile.Trimesh m in colmesh.Meshes) {
					// Creating affine transformation
					// Создание трансформации
					BEPUutilities.AffineTransform transform = new BEPUutilities.AffineTransform(
						new BEPUutilities.Vector3(scale.X, scale.Y, scale.Z),
						new BEPUutilities.Quaternion(angles.X, angles.Y, angles.Z, angles.W),
						new BEPUutilities.Vector3(position.X, position.Y, position.Z)
					);

					// Create primitive
					// Создание примитива
					col.Add(new CollisionEntry() {
						Type = PrimitiveType.Mesh,
						Mesh = new StaticMesh(verts, m.Indices, transform)
					});
				}
			}
			subColliders = col.ToArray();
		}

		/// <summary>
		/// Enable collider physics<para/>
		/// Активация физики коллайдера
		/// </summary>
		public void Enable() {
			if (!IsEnabled) {
				foreach (CollisionEntry e in subColliders) {
					if (e.Type == PrimitiveType.Mesh) {
						PhysicsManager.World.Add(e.Mesh);
					}else{
						PhysicsManager.World.Add(e.Body);
						e.Body.IsAffectedByGravity = false;
						e.Body.Position = new BEPUutilities.Vector3(
							e.Position.X, e.Position.Y, e.Position.Z	
						);
						e.Body.Orientation = new BEPUutilities.Quaternion(
							e.Rotation.X, e.Rotation.Y, e.Rotation.Z, e.Rotation.W
						);
					}
				}
				IsEnabled = true;
			}
		}

		/// <summary>
		/// Disable collider physics<para/>
		/// Деактивация физики коллайдера
		/// </summary>
		public void Disable() {
			if (IsEnabled) {
				foreach (CollisionEntry e in subColliders) {
					if (e.Type == PrimitiveType.Mesh) {
						PhysicsManager.World.Remove(e.Mesh);
					} else {
						PhysicsManager.World.Remove(e.Body);
					}
				}
				IsEnabled = false;
			}
		}

		/// <summary>
		/// Single collision primitive<para/>
		/// Один примитив для коллайдера
		/// </summary>
		class CollisionEntry {
			/// <summary>
			/// Type of this primitive<para/>
			/// Тип данного примитива
			/// </summary>
			public PrimitiveType Type;

			/// <summary>
			/// Body position in world coordinates<para/>
			/// Расположение тела в мире
			/// </summary>
			public Vector3 Position;

			/// <summary>
			/// Body rotation in world space<para/>
			/// Углы поворота тела в пространстве
			/// </summary>
			public Quaternion Rotation;

			/// <summary>
			/// Physical body<para/>
			/// Физическое тело
			/// </summary>
			public Entity Body;

			/// <summary>
			/// Static mesh collider if body is mesh<para/>
			/// Статичный меш-коллайдер
			/// </summary>
			public StaticMesh Mesh;

			/// <summary>
			/// Collision shape for this body<para/>
			/// Данные коллизии для этого тела
			/// </summary>
			public EntityShape Shape;
		}

		/// <summary>
		/// Collision primitive type<para/>
		/// Тип примитива столкновений
		/// </summary>
		enum PrimitiveType {
			Sphere,
			Box,
			Mesh
		}
	}
}
