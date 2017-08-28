namespace Mapbox.Unity.MeshGeneration
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Mapbox.Map;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Unity.MeshGeneration.Data;
	using System;
	using Mapbox.Unity.Map;
	using Mapbox.Platform;

	public /*abstract*/ class AbstractMapVisualizer : ScriptableObject, IMapVisualizer
	{
		[SerializeField]
		public AbstractTileFactory[] _factories { get; set; }

		public IMap _map { get; internal set; }

		public Queue<UnityTile> _inactiveTiles { get; set; }


		public Dictionary<UnwrappedTileId, UnityTile> Tiles { get; set; }

		private ModuleState _state;
		public ModuleState State
		{
			get
			{
				return _state;
			}
			internal set
			{
				if (_state != value)
				{
					_state = value;
					OnMapVisualizerStateChanged(_state);
				}
			}
		}

		public event Action<ModuleState> OnMapVisualizerStateChanged = delegate { };

		/// <summary>
		/// Initializes the factories by passing the file source down, which's necessary for data (web/file) calls
		/// </summary>
		/// <param name="fileSource"></param>
		public void Initialize(IMap map, IFileSource fileSource)
		{
			_map = map;
			Tiles = new Dictionary<UnwrappedTileId, UnityTile>();
			_inactiveTiles = new Queue<UnityTile>();
			State = ModuleState.Initialized;

			foreach (var factory in _factories)
			{
				factory.Initialize(fileSource);
				factory.OnFactoryStateChanged += UpdateState;
			}
		}

		public void Destroy()
		{
			for (int i = 0; i < _factories.Length; i++)
			{
				if (_factories[i] != null)
					_factories[i].OnFactoryStateChanged -= UpdateState;
			}
		}


		internal void UpdateState(AbstractTileFactory factory)
		{
			if (State != ModuleState.Working && factory.State == ModuleState.Working)
			{
				State = ModuleState.Working;
			}
			else if (State != ModuleState.Finished && factory.State == ModuleState.Finished)
			{
				var allFinished = true;
				for (int i = 0; i < _factories.Length; i++)
				{
					if (_factories[i] != null)
					{
						allFinished &= _factories[i].State == ModuleState.Finished;
					}
				}
				if (allFinished)
				{
					State = ModuleState.Finished;
				}
			}
		}


		/// <summary>
		/// Registers requested tiles to the factories
		/// </summary>
		/// <param name="tileId"></param>
		public UnityTile LoadTile(UnwrappedTileId tileId)
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

			unityTile.Initialize(_map, tileId);

			foreach (var factory in _factories)
			{
				factory.Register(unityTile);
			}

			Tiles.Add(tileId, unityTile);

			return unityTile;
		}

		public void DisposeTile(UnwrappedTileId tileId)
		{
			var unityTile = Tiles[tileId];

			unityTile.Recycle();
			Tiles.Remove(tileId);
			_inactiveTiles.Enqueue(unityTile);

			foreach (var factory in _factories)
			{
				factory.Unregister(unityTile);
			}
		}




	}
}