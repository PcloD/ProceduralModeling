using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralModeling {

	public class Tubular : ProceduralModelingBase {

		// CurveBaseはScriptableObjectであるため、asset化したものを指定する
		[SerializeField] protected CurveBase curve;

		[SerializeField, Range(2, 50)] protected int tubularSegments = 20, radialSegments = 8;
		[SerializeField, Range(0.1f, 5f)] protected float radius = 0.5f;
		[SerializeField] protected bool closed = false;

		const float PI2 = Mathf.PI * 2f;

		protected override Mesh Build() {
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var tangents = new List<Vector4>();
			var uvs = new List<Vector2>();
			var triangles = new List<int>();

			if(curve == null) {
				return new Mesh();
			}

			// 曲線からFrenet frameを取得
			var frames = curve.ComputeFrenetFrames(tubularSegments, closed);

			// Tubularの頂点データを生成
			for(int i = 0; i < tubularSegments; i++) {
				GenerateSegment(curve, frames, vertices, normals, tangents, i);
			}
			// 閉じた筒型を生成する場合は曲線の始点に最後の頂点を配置し、閉じない場合は曲線の終点に配置する
			GenerateSegment(curve, frames, vertices, normals, tangents, (!closed) ? tubularSegments : 0);

			// 曲線の始点から終点に向かってuv座標を設定していく
			for (int i = 0; i <= tubularSegments; i++) {
				for (int j = 0; j <= radialSegments; j++) {
					float u = 1f * j / radialSegments;
					float v = 1f * i / tubularSegments;
					uvs.Add(new Vector2(u, v));
				}
			}

			// 側面を構築
			for (int j = 1; j <= tubularSegments; j++) {
				for (int i = 1; i <= radialSegments; i++) {
					int a = (radialSegments + 1) * (j - 1) + (i - 1);
					int b = (radialSegments + 1) * j + (i - 1);
					int c = (radialSegments + 1) * j + i;
					int d = (radialSegments + 1) * (j - 1) + i;

					triangles.Add(a); triangles.Add(d); triangles.Add(b);
					triangles.Add(b); triangles.Add(d); triangles.Add(c);
				}
			}

			var mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.normals = normals.ToArray();
			mesh.tangents = tangents.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.triangles = triangles.ToArray();
			return mesh;
		}

		void GenerateSegment(CurveBase curve, List<FrenetFrame> frames, List<Vector3> vertices, List<Vector3> normals, List<Vector4> tangents, int index) {
			// 0.0 ~ 1.0
			var u = 1f * index / tubularSegments;

			var p = curve.GetPointAt(u);
			var fr = frames[index];

			var N = fr.Normal;
			var B = fr.Binormal;

			for(int j = 0; j <= radialSegments; j++) {
				// 0.0 ~ 2π
				float rad = 1f * j / radialSegments * PI2;

				// 円周に沿って均等に頂点を配置する
				float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
				var normal = (cos * N + sin * B).normalized;
				vertices.Add(p + radius * normal);
				normals.Add(normal);

				var tangent = fr.Tangent;
				tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, 0f));
			}
		}

	}

}

