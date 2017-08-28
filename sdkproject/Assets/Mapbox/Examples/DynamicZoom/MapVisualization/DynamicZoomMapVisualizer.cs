namespace Mapbox.Unity.Examples.DynamicZoom
{
	using Mapbox.Map;
	using System.Collections.Generic;
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Platform;
	using Mapbox.Unity.Map;
	using System;
	using Mapbox.Unity.MeshGeneration;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;

	[CreateAssetMenu(menuName = "Mapbox/DynamicZoomMapVisualizer")]
	public class DynamicZoomMapVisualizer : AbstractMapVisualizer
	{



		/// <summary>
		/// Registers requested tiles to the factories
		/// </summary>
		/// <param name="tileId"></param>
		public override UnityTile LoadTile(UnwrappedTileId tileId)
		{
			UnityTile unityTile = null;

			if (_inactiveTiles.Count > 0)
			{
				unityTile = _inactiveTiles.Dequeue();
			}

			if (unityTile == null)
			{
				unityTile = new GameObject().AddComponent<UnityTile>();
				unityTile.transform.SetParent(_map.Root, false);
			}

			//HACK: switch COORDINATES - there's a bug somewhere with switched x<->y
			Vector2d centerWebMercDUMMY = new Vector2d(_map.CenterMercator.y, _map.CenterMercator.x);
			UnwrappedTileId centerTile = TileCover.WebMercatorToTileId(centerWebMercDUMMY, _map.Zoom);
			Vector2d centerTileCenter = Conversions.TileIdToCenterWebMercator(centerTile.X, centerTile.Y, _map.Zoom);
			Vector2d shift = _map.CenterMercator - centerTileCenter;
			//float factor = Conversions.GetTileScaleInMeters(_map.Zoom) * 256 / ((DynamicZoomMap)_map).UnityTileSize;
			int unityTileSize = ((DynamicZoomMap)_map).UnityTileSize;
			float factor = Conversions.GetTileScaleInMeters((float)_map.CenterLatitudeLongitude.x, _map.Zoom) * 256 / unityTileSize;
			Vector3 unityTileScale = new Vector3(unityTileSize, 1, unityTileSize);

			//position the tile relative to the center tile of the current viewport using the tile id
			//multiply by tile size Unity units (unityTileScale)
			//shift by distance of current viewport center to center of center tile
			Vector3 position = new Vector3(
				(tileId.X - centerTile.X) * unityTileSize - (float)shift.x / factor
				, 0
				, (centerTile.Y - tileId.Y) * unityTileSize - (float)shift.y / factor
			);

			unityTile.Initialize(_map, tileId, position, unityTileScale);

			foreach (var factory in _factories)
			{
				factory.Register(unityTile);
			}

			Tiles.Add(tileId, unityTile);

			return unityTile;
		}



	}
}