namespace Mapbox.Unity.Examples.DynamicZoom
{
	using Mapbox.Unity.MeshGeneration.Factories;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Map;
	using Mapbox.Unity.MeshGeneration.Enums;
	using Mapbox.Unity.Utilities;

	[CreateAssetMenu(menuName = "Mapbox/Factories/Dynamic Zoom Image Factory")]

	public class DynamicZoomImageFactory : AbstractTileFactory
	{

		[SerializeField]
		private MapImageType _mapIdType;

		[SerializeField]
		private string _mapId = "";

		private string _className;

		internal override void OnInitialized()
		{
			_className = this.GetType().Name;

			Debug.LogFormat("{0}.{1}", _className, new System.Diagnostics.StackFrame().GetMethod().Name);
		}


		internal override void OnRegistered(UnityTile tile)
		{
			Debug.LogFormat("{0}.{1} tile:{2}", _className, new System.Diagnostics.StackFrame().GetMethod().Name, tile.CanonicalTileId);

			if (null == tile.MeshRenderer) { tile.gameObject.AddComponent<MeshRenderer>(); }
			if (tile.MeshFilter == null) { tile.gameObject.AddComponent<MeshFilter>(); }
			tile.MeshFilter.sharedMesh = BuildQuad(tile);

			tile.RasterDataState = TilePropertyState.Loading;
			RasterTile rasterTile = new RasterTile();
			tile.AddTile(rasterTile);
			rasterTile.Initialize(_fileSource, tile.CanonicalTileId, _mapId, () =>
			{
				if (rasterTile.HasError)
				{
					tile.RasterDataState = TilePropertyState.Error;
					Debug.LogErrorFormat("{0}.{1}:{2}", _className, new System.Diagnostics.StackFrame().GetMethod().Name, rasterTile.ExceptionsAsString);
					return;
				}

				tile.SetRasterData(rasterTile.Data, false, false);
				tile.RasterDataState = TilePropertyState.Loaded;
			});
		}


		internal override void OnUnregistered(UnityTile tile) { }



		Mesh BuildQuad(UnityTile tile)
		{
			var unityMesh = new Mesh();
			var verts = new Vector3[4];

			verts[0] = ((tile.Rect.Min - tile.Rect.Center).ToVector3xz());
			verts[2] = (new Vector3((float)(tile.Rect.Min.x - tile.Rect.Center.x), 0, (float)(tile.Rect.Max.y - tile.Rect.Center.y)));
			verts[1] = (new Vector3((float)(tile.Rect.Max.x - tile.Rect.Center.x), 0, (float)(tile.Rect.Min.y - tile.Rect.Center.y)));
			verts[3] = ((tile.Rect.Max - tile.Rect.Center).ToVector3xz());

			Debug.LogFormat("verts0:{0} verts2:{1} verts1:{2} verts3:{3}", verts[0], verts[2], verts[1], verts[3]);

			verts[0] = new Vector3(-0.5f, 0, 0.5f);
			verts[2] = new Vector3(-0.5f, 0, -0.5f);
			verts[1] = new Vector3(0.5f, 0, 0.5f);
			verts[3] = new Vector3(0.5f, 0, -0.5f);

			//verts[0] = new Vector3(0, 0, 1);
			//verts[2] = new Vector3(0, 0, 0);
			//verts[1] = new Vector3(1, 0, 1);
			//verts[3] = new Vector3(1, 0, 0);


			unityMesh.vertices = verts;
			var trilist = new int[6] { 0, 1, 2, 1, 3, 2 };
			unityMesh.SetTriangles(trilist, 0);
			var uvlist = new Vector2[4]
			{
				new Vector2(0,1),
				new Vector2(1,1),
				new Vector2(0,0),
				new Vector2(1,0)
			};

			unityMesh.uv = uvlist;
			unityMesh.RecalculateNormals();

			tile.MeshFilter.sharedMesh = unityMesh;

			return unityMesh;
		}


	}
}
