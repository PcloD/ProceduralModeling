using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralModeling {

	public class Quad : ProceduralModelingBase {

		[SerializeField, Range(0.1f, 10f)] protected float size = 1f;

		protected override Mesh Build() {
			var mesh = new Mesh();

			// Quadの横幅と縦幅がそれぞれsizeの長さになるように半分の長さを求める
			var hsize = size * 0.5f; 

			// Quadの頂点データ
			var vertices = new Vector3[] {
				new Vector3(-hsize,  hsize, 0f), // 0番目の頂点 Quadの左上の位置
				new Vector3( hsize,  hsize, 0f), // 1番目の頂点 Quadの右上の位置
				new Vector3( hsize, -hsize, 0f), // 2番目の頂点 Quadの右下の位置
				new Vector3(-hsize, -hsize, 0f)  // 3番目の頂点 Quadの左下の位置
			};

			// Quadのuv座標データ
			var uv = new Vector2[] {
				new Vector2(0f, 0f),
				new Vector2(1f, 0f),
				new Vector2(1f, 1f),
				new Vector2(0f, 1f)
			};

			// Quadの法線データ
			var normals = new Vector3[] {
				new Vector3(0f, 0f, -1f),
				new Vector3(0f, 0f, -1f),
				new Vector3(0f, 0f, -1f),
				new Vector3(0f, 0f, -1f)
			};

			// Quadの面データ 頂点のindexを3つ並べて1つの面(三角形)として認識する
			var indices = new int[] {
				0, 1, 2,
				2, 3, 0
			};

			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.normals = normals;
			mesh.SetIndices(indices, MeshTopology.Triangles, 0);

			return mesh;
		}

	}

}

