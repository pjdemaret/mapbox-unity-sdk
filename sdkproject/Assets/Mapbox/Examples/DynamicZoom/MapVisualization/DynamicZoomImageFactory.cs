namespace Mapbox.Unity.Examples.DynamicZoom
{
	using Mapbox.Unity.MeshGeneration.Factories;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Map;
	using Mapbox.Unity.MeshGeneration.Enums;

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




	}
}
