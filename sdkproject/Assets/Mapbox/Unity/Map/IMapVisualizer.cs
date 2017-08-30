namespace Mapbox.Map
{
	using Mapbox.Platform;
	using Mapbox.Unity.Map;
	using Mapbox.Unity.MeshGeneration;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.MeshGeneration.Factories;
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public interface IMapVisualizer
	{

		IMap _map { get; }
		Queue<UnityTile> InactiveTiles { get; }

		AbstractTileFactory[] Factories { get; }
		Dictionary<UnwrappedTileId, UnityTile> Tiles { get; }

		ModuleState State { get; }

		event Action<ModuleState> OnMapVisualizerStateChanged;

		void Initialize(IMap map, IFileSource fileSource);

		void Destroy();

		UnityTile LoadTile(UnwrappedTileId tileId);

		void DisposeTile(UnwrappedTileId tileId);
	}


}