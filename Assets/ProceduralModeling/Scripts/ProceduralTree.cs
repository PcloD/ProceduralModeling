using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ProceduralModeling {

	public class ProceduralTree : ProceduralModelingBase {

		[SerializeField] TreeData data;
		[SerializeField, Range(2, 8)] protected int generations = 5;
		[SerializeField, Range(0.5f, 5f)] protected float length = 1f;
		[SerializeField, Range(0.1f, 2f)] protected float radius = 0.15f;

		TreeBranch root;

		const float PI2 = Mathf.PI * 2f;

		protected override Mesh Build ()
		{
			data.Setup();
			root = new TreeBranch(
				generations, 
				length, 
				radius, 
				data
			);

			var mesh = new Mesh();

			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var tangents = new List<Vector4>();
			var uvs = new List<Vector2>();
			var triangles = new List<int>();

			float maxLength = TraverseMaxLength(root);

			Traverse(root, (branch) => {
				var offset = vertices.Count;

				var vOffset = branch.Offset / maxLength;
				var vLength = branch.Length / maxLength;

				for(int i = 0, n = branch.Segments.Count; i < n; i++) {
					var t = 1f * i / (n - 1);
					var v = vOffset + vLength * t;

					var segment = branch.Segments[i];
					var N = segment.Frame.Normal;
					var B = segment.Frame.Binormal;
					for(int j = 0; j <= data.RadialSegments; j++) {
						// 0.0 ~ 2π
						var u = 1f * j / data.RadialSegments;
						float rad = u * PI2;

						// 円周に沿って均等に頂点を配置する
						float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
						var normal = (cos * N + sin * B).normalized;
						vertices.Add(segment.Position + Mathf.Lerp(branch.FromRadius, branch.ToRadius, t) * normal);
						normals.Add(normal);

						var tangent = segment.Frame.Tangent;
						tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, 0f));

						uvs.Add(new Vector2(u, v));
					}
				}

				// 側面を構築
				for (int j = 1; j <= data.HeightSegments; j++) {
					for (int i = 1; i <= data.RadialSegments; i++) {
						int a = (data.RadialSegments + 1) * (j - 1) + (i - 1);
						int b = (data.RadialSegments + 1) * j + (i - 1);
						int c = (data.RadialSegments + 1) * j + i;
						int d = (data.RadialSegments + 1) * (j - 1) + i;

						a += offset;
						b += offset;
						c += offset;
						d += offset;

						triangles.Add(a); triangles.Add(d); triangles.Add(b);
						triangles.Add(b); triangles.Add(d); triangles.Add(c);
					}
				}
			});

			mesh.vertices = vertices.ToArray();
			mesh.normals = normals.ToArray();
			mesh.tangents = tangents.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.triangles = triangles.ToArray();

			return mesh;
		}

		protected override Material LoadMaterial(ProceduralModelingMaterial type) {
			switch(type) {
			case ProceduralModelingMaterial.UV:
				return Resources.Load<Material>("Materials/ProceduralTreeUV");
			case ProceduralModelingMaterial.Normal:
				return Resources.Load<Material>("Materials/ProceduralTreeNormal");
			}
			return Resources.Load<Material>("Materials/ProceduralTreeStandard");
		}

		float TraverseMaxLength(TreeBranch branch) {
			float max = 0f;
			branch.Children.ForEach(c => {
				max = Mathf.Max(max, TraverseMaxLength(c));
			});
			return branch.Length + max;
		}

		void OnDrawGizmos () {
			if(root == null) return;

			Gizmos.matrix = transform.localToWorldMatrix;

			const float size = 0.01f;
			const float dsize = size * 2f;

			var inv = 1f / (generations - 1);
			Traverse(root, (TreeBranch b) => {
				var r = (1f - b.Generation * inv);

				var color = new Color(0.0f, r, 0.1f * r);
				DrawSegmentGizmos(b, color);
			});
		}

		void DrawSegmentGizmos (TreeBranch branch, Color color) {
			for(int i = 0, n = branch.Segments.Count - 1; i < n; i++) {
				var s0 = branch.Segments[i];
				var s1 = branch.Segments[i + 1];
				Gizmos.color = color;
				Gizmos.DrawLine(s0.Position, s1.Position);
			}
		}

		void Traverse(TreeBranch from, Action<TreeBranch> action) {
			if(from.Children.Count > 0) {
				from.Children.ForEach(child => {
					Traverse(child, action);
				});
			}
			action(from);
		}

	}

	[System.Serializable]
	public class TreeData {
		public float LengthAttenuation { get { return lengthAttenuation; } }
		public float RadiusAttenuation { get { return radiusAttenuation; } }

		public int HeightSegments { get { return heightSegments; } }
		public int RadialSegments { get { return radialSegments; } }

		[SerializeField] protected int randomSeed = 0;
		[SerializeField, Range(0.5f, 1f)] protected float lengthAttenuation = 0.86f, radiusAttenuation = 0.7f;
		[SerializeField, Range(1, 3)] protected int branchesMin = 1, branchesMax = 3;
		[SerializeField, Range(-90f, 90f)] protected float angleMin = -40f, angleMax = 40f;
		[SerializeField, Range(4, 20)] protected int heightSegments = 10, radialSegments = 8;
		[SerializeField, Range(0.0f, 0.35f)] protected float bendDegree = 0.1f;

		Rand rnd;

		public void Setup() {
			rnd = new Rand(randomSeed);
		}

		public int Range(int a, int b) {
			return rnd.Range(a, b);
		}

		public int GetRandomBranches() {
			return rnd.Range(branchesMin, branchesMax + 1);
		}

		public float GetRandomAngle() {
			return rnd.Range(angleMin, angleMax);
		}

		public float GetRandomBendDegree() {
			return rnd.Range(-bendDegree, bendDegree);
		}
	}

	public class TreeBranch {
		public int Generation { get { return generation; } }
		public List<TreeSegment> Segments { get { return segments; } }
		public List<TreeBranch> Children { get { return children; } }

		public Vector3 From { get { return from; } }
		public Vector3 To { get { return to; } }
		public float Length { get { return length; } } 
		public float Offset { get { return offset; } }

		public float FromRadius { get { return fromRadius; } }
		public float ToRadius { get { return toRadius; } }

		int generation;

		List<TreeSegment> segments;
		List<TreeBranch> children;

		Vector3 from, to;
		float fromRadius, toRadius;
		float length;
		float offset;

		// for Root branch constructor
		public TreeBranch(int generation, float length, float radius, TreeData data) : this(generation, Vector3.zero, Vector3.up, Vector3.right, Vector3.back, length, radius, 0f, data) {
		}

		protected TreeBranch(int generation, Vector3 from, Vector3 tangent, Vector3 normal, Vector3 binormal, float length, float radius, float offset, TreeData data) {
			this.generation = generation;

			this.fromRadius = radius;
			this.toRadius = (generation == 0) ? 0f : radius * data.RadiusAttenuation;

			this.from = from;

			var rotation = Quaternion.AngleAxis(data.GetRandomAngle(), normal) * Quaternion.AngleAxis(data.GetRandomAngle(), binormal);
			this.to = from + rotation * tangent * length;
			this.length = length;
			this.offset = offset;

			segments = BuildSegments(data, radius, normal, binormal);

			children = new List<TreeBranch>();
			if(generation > 0) {
				int branches = data.GetRandomBranches();
				for(int i = 0; i < branches; i++) {
					// 一番初めの枝は一続きの枝にする
					bool sequence = (i == 0);

					int index = sequence ? segments.Count - 1 : data.Range(1, segments.Count - 1);
					var ratio = 1f * index / (segments.Count - 1);

					var segment = segments[index];
					var nt = segment.Frame.Tangent;
					var nn = segment.Frame.Normal;
					var nb = segment.Frame.Binormal;

					var child = new TreeBranch(
						this.generation - 1, 
						segment.Position, 
						nt, 
						nn, 
						nb, 
						length * Mathf.Lerp(1f, data.LengthAttenuation, ratio), 
						radius * Mathf.Lerp(1f, data.RadiusAttenuation, ratio),
						offset + length,
						data
					);

					children.Add(child);
				}
			}
		}

		List<TreeSegment> BuildSegments (TreeData data, float radius, Vector3 normal, Vector3 binormal) {
			var segments = new List<TreeSegment>();

			var curve = ScriptableObject.CreateInstance<CatmullRomCurve>();

			var length = (to - from).magnitude;
			var bend = length * (normal * data.GetRandomBendDegree() + binormal * data.GetRandomBendDegree());
			curve.Points[0] = from;
			curve.Points[1] = Vector3.Lerp(from, to, 0.25f) + bend;
			curve.Points[2] = Vector3.Lerp(from, to, 0.75f) + bend;
			curve.Points[3] = to;

			var frames = curve.ComputeFrenetFrames(data.HeightSegments, normal, binormal, false);
			// var frames = curve.ComputeFrenetFrames(data.HeightSegments, false);
			for(int i = 0, n = frames.Count; i < n; i++) {
				var u = 1f * i / (n - 1);
				var position = curve.GetPointAt(u);
				var segment = new TreeSegment(frames[i], position);
				segments.Add(segment);
			}
			return segments;
		}
		
	}

	public class TreeSegment {
		public FrenetFrame Frame { get { return frame; } }
		public Vector3 Position { get { return position; } }

		FrenetFrame frame;
		Vector3 position;

		public TreeSegment(FrenetFrame frame, Vector3 position) {
			this.frame = frame;
			this.position = position;
		}
	}

	public class Rand {
		System.Random rnd;

		public float value {
			get {
				return (float)rnd.NextDouble();
			}
		}

		public Rand(int seed) {
			rnd = new System.Random(seed);
		}

		public int Range(int a, int b) {
			var v = value;
			return Mathf.FloorToInt(Mathf.Lerp(a, b, v));
		}

		public float Range(float a, float b) {
			var v = value;
			return Mathf.Lerp(a, b, v);
		}
	}

}

