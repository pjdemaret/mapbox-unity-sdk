namespace Mapbox.Unity.MeshGeneration.Modifiers
{
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Components;
	using Mapbox.Unity.MeshGeneration.Interfaces;
	using System;
	using System.Collections.Generic;
	using Mapbox.Unity.MeshGeneration.Data;

	public enum LineDistributionType
	{
		FixedInterval,
		FixedCount,
		Random
	}


	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Line Distribution Modifier")]
	public class LineDistributionModifier : GameObjectModifier
	{
		private Transform _parent;

		[SerializeField]
		private LineDistributionType _type;

		//fixed interval
		[SerializeField]
		private float _intervalDistance;
		[SerializeField]
		private bool _placeAtStart;
		[SerializeField]
		private bool _placeAtEnd;

		//fixed count
		[SerializeField]
		private float _pointCount;

		//random distribution
		[SerializeField]
		private float _minDistance;
		[SerializeField]
		private float _maxDistance;

		[SerializeField]
		private bool _centralizePoints;

		[SerializeField]
		private GameObject _prefab;

		[SerializeField]
		private List<PlacerBase> _placers;


		private float dist, stepDistance;
		private int count;
		private Vector3 dif, dir, startPoint;
		[NonSerialized]
		private List<Vector3> _positions;
		[NonSerialized]
		private List<Quaternion> _rotations;
		
		public override void Run(FeatureBehaviour fb)
		{
			_positions = new List<Vector3>();
			_rotations = new List<Quaternion>();

			_parent = fb.transform;
			if (_prefab == null)
				_prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

			foreach (var segment in fb.Data.Points)
			{
				for (int j = 0; j < segment.Count - 1; j++)
				{
					switch (_type)
					{
						case LineDistributionType.FixedInterval:
							FixedInterval(fb, segment[j], segment[j + 1]);
							break;
						case LineDistributionType.FixedCount:
							FixedCount(fb, segment[j], segment[j + 1]);
							break;
						case LineDistributionType.Random:
							RandomDistribution(fb, segment[j], segment[j + 1]);
							break;
						default:
							break;
					}
				}
			}

			for (int i = 0; i < _positions.Count; i++)
			{
				var go = Instantiate(_prefab);
				go.name = name;
				go.transform.SetParent(_parent, false);
				go.transform.localPosition = _positions[i] + Vector3.up * 1.5f;
				go.transform.rotation = _rotations[i];
			}
		}

		private void RandomDistribution(FeatureBehaviour fb, Vector3 f, Vector3 s)
		{
			dif = (s - f);
			dist = dif.magnitude;
			dir = dif.normalized;

			startPoint = f;
			if (_placeAtStart)
				CreateObject(startPoint, dir, fb.Data);

			var delta = 0f;
			delta += UnityEngine.Random.Range(_minDistance, _maxDistance);
			while (delta < dist)
			{
				CreateObject(startPoint + dir * delta, dir, fb.Data);
				delta += UnityEngine.Random.Range(_minDistance, _maxDistance);
			} 

			if (_placeAtEnd)
				CreateObject(s, dir, fb.Data);
		}

		private void FixedCount(FeatureBehaviour fb, Vector3 f, Vector3 s)
		{
			dif = (s - f);
			dist = dif.magnitude;
			dir = dif.normalized;
			stepDistance = dist / (_pointCount + 2);

			startPoint = f;

			if (_placeAtStart)
				CreateObject(startPoint, dir, fb.Data);

			if (_centralizePoints)
			{
				startPoint += dir * (dist - (_pointCount * stepDistance)) / 2;
				CreateObject(startPoint, dir, fb.Data);
			}

			for (int i = 1; i <= _pointCount; i++)
			{
				CreateObject(startPoint + dir * stepDistance * i, dir, fb.Data);
			}

			if (_placeAtEnd)
				CreateObject(s, dir, fb.Data);
		}
		
		private void FixedInterval(FeatureBehaviour fb, Vector3 f, Vector3 s)
		{
			dif = (s - f);
			dist = dif.magnitude;
			dir = dif.normalized;
			count = (int)((dist-10) / _intervalDistance);

			startPoint = f;

			if (_placeAtStart)
				CreateObject(startPoint, dir, fb.Data);

			if (_centralizePoints)
			{
				startPoint += dir * (dist - (count * _intervalDistance)) / 2;
				CreateObject(startPoint, dir, fb.Data);
			}

			for (int i = 1; i <= count; i++)
			{
				CreateObject(startPoint + dir * _intervalDistance * i, dir, fb.Data);
			}

			if (_placeAtEnd)
				CreateObject(s, dir, fb.Data);
		}

		private void CreateObject(Vector3 position, Vector3 dir, VectorFeatureUnity feature)
		{
			foreach (var placer in _placers)
			{
				placer.Run(_positions, _rotations, feature, position, dir);
			}			
		}
	}
}
