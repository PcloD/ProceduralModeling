using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralModeling {

	public class Cylinder : ProceduralModelingBase {

		[SerializeField, Range(0.1f, 10f)] protected float height = 3f, radius = 1f;
		[SerializeField, Range(3, 32)] protected int segments = 16;
		[SerializeField] bool openEnded = true;

		protected override Mesh Build() {
			var mesh = new Mesh();

			var hh = height * 0.5f;
			var vertices = new List<Vector3>();
			var uvs = new List<Vector2>();
			var triangles = new List<int>();

			// 側面の三角形を構築する際、円上の頂点を参照するために、
			// indexが円を一周するための除数
			var len = segments * 2;

			// 円を形作るための2π
			var pi2 = Mathf.PI * 2f;

			for (int i = 0; i < segments; i++) {
				// 0.0 ~ 1.0
				float ratio = (float)i / (segments - 1);

				// 0.0 ~ 2π
				float rad = ratio * pi2;

				// 円に沿って上端と下端に均等に頂点を配置する
				var top = new Vector3(Mathf.Cos(rad) * radius, hh, Mathf.Sin(rad) * radius);
				var bottom = new Vector3(Mathf.Cos(rad) * radius, - hh, Mathf.Sin(rad) * radius);

				// 上端
				vertices.Add(top); 
				uvs.Add(new Vector2(ratio, 1f));

				// 下端
				vertices.Add(bottom); 
				uvs.Add(new Vector2(ratio, 0f));

				// 上端と下端をつなぎ合わせて側面を構築
				int idx = i * 2;
				int a = idx, b = idx + 1, c = (idx + 2) % len, d = (idx + 3) % len;
				triangles.Add(a);
				triangles.Add(c);
				triangles.Add(b);

				triangles.Add(c);
				triangles.Add(d);
				triangles.Add(b);
			}

			if(openEnded) {
				vertices.Add(new Vector3(0f, hh, 0f)); // top
				uvs.Add(new Vector2(0.5f, 1f));

				vertices.Add(new Vector3(0f, -hh, 0f)); // bottom
				uvs.Add(new Vector2(0.5f, 0f));

				var top = vertices.Count - 2;
				var bottom = vertices.Count - 1;

				int n = segments * 2;

				// top side
				for (int i = 0; i < n; i += 2) {
					triangles.Add(top);
					triangles.Add((i + 2) % n);
					triangles.Add(i);
				}

				// bottom side
				for (int i = 1; i < n; i += 2) {
					triangles.Add(bottom);
					triangles.Add(i);
					triangles.Add((i + 2) % n);
				}
			}

			mesh.vertices = vertices.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			return mesh;
		}

	}

}

