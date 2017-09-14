using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralModeling {

	public class Plane : ProceduralModelingBase {

		// Planeの列数(widthSegments)と行数(heightSegments)
		[SerializeField, Range(2, 30)] protected int widthSegments = 8, heightSegments = 8;

		// Planeの横幅と縦幅
		[SerializeField, Range(0.1f, 10f)] protected float width = 1f, height = 1f;

		protected override Mesh Build() {
			var mesh = new Mesh();

			var vertices = new Vector3[heightSegments * widthSegments];
			var uv = new Vector2[heightSegments * widthSegments];
			var normals = new Vector3[heightSegments * widthSegments];

			var hwidth = width * 0.5f; 
			var hheight = height * 0.5f; 

			// 頂点のグリッド上での位置の割合(0.0 ~ 1.0)を算出するための行列数の逆数
			var winv = 1f / (widthSegments - 1);
			var hinv = 1f / (heightSegments - 1);

			for(int y = 0; y < heightSegments; y++) {
				// 行の位置の割合(0.0 ~ 1.0)
				var ry = y * hinv;

				for(int x = 0; x < widthSegments; x++) {
					// 列の位置の割合(0.0 ~ 1.0)
					var rx = x * winv;

					int index = y * widthSegments + x;

					vertices[index] = new Vector3(
						(rx - 0.5f) * width, 
						0f,
						(0.5f - ry) * height
					);
					uv[index] = new Vector3(rx, ry);
					normals[index] = new Vector3(0f, 1f, 0f);
				}
			}

			// Quadを1つ構築するのに、頂点indexは6つ必要
			// よって縦に(heightSegments - 1)個、横に(widthSegments - 1)個分のQuadを並べるには
			// (6 * (heightSegments - 1) * (widthSegments - 1))つの頂点indexが必要
			var triangles = new int[6 * (heightSegments - 1) * (widthSegments - 1)];

			for(int y = 0; y < heightSegments - 1; y++) {
				var tyOffset = 6 * y * (widthSegments - 1);
				for(int x = 0; x < widthSegments - 1; x++) {
					var txOffset = x * 6;
					var tOffset = tyOffset + txOffset;

					int index = y * widthSegments + x;
					var a = index;
					var b = index + 1;
					var c = index + 1 + widthSegments;
					var d = index + widthSegments;

					triangles[tOffset] 		= a;
					triangles[tOffset + 1] 	= b;
					triangles[tOffset + 2] 	= c;

					triangles[tOffset + 3] 	= c;
					triangles[tOffset + 4] 	= d;
					triangles[tOffset + 5] 	= a;
				}
			}

			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.normals = normals;
			mesh.triangles = triangles;

			mesh.RecalculateBounds();

			return mesh;
		}

	}

}

