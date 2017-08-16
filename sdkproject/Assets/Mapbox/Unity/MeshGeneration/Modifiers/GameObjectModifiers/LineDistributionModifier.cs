namespace Mapbox.Unity.MeshGeneration.Modifiers
{
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Components;
	using Mapbox.Unity.MeshGeneration.Interfaces;
	using System;

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


		private float dist, stepDistance;
		private int count;
		private Vector3 dif, dir, startPoint;

		public override void Run(FeatureBehaviour fb)
		{
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
		}

		private void RandomDistribution(FeatureBehaviour fb, Vector3 f, Vector3 s)
		{
			dif = (s - f);
			dist = dif.magnitude;
			dir = dif.normalized;

			startPoint = f;
			if (_placeAtStart)
				CreateObject(startPoint, "start");

			var delta = 0f;
			delta += UnityEngine.Random.Range(_minDistance, _maxDistance);
			while (delta < dist)
			{
				CreateObject(startPoint + dir * delta);
				delta += UnityEngine.Random.Range(_minDistance, _maxDistance);
			} 

			if (_placeAtEnd)
				CreateObject(s, "end");
		}

		private void FixedCount(FeatureBehaviour fb, Vector3 f, Vector3 s)
		{
			dif = (s - f);
			dist = dif.magnitude;
			dir = dif.normalized;
			stepDistance = dist / (_pointCount + 2);

			startPoint = f;

			if (_placeAtStart)
				CreateObject(startPoint, "start");

			if (_centralizePoints)
			{
				startPoint += dir * (dist - (_pointCount * stepDistance)) / 2;
				CreateObject(startPoint);
			}

			for (int i = 1; i <= _pointCount; i++)
			{
				CreateObject(startPoint + dir * stepDistance * i);
			}

			if (_placeAtEnd)
				CreateObject(s, "end");
		}


		private void FixedInterval(FeatureBehaviour fb, Vector3 f, Vector3 s)
		{
			dif = (s - f);
			dist = dif.magnitude;
			dir = dif.normalized;
			count = (int)(dist / _intervalDistance);

			startPoint = f;

			if (_placeAtStart)
				CreateObject(startPoint, "start");

			if (_centralizePoints)
			{
				startPoint += dir * (dist - (count * _intervalDistance)) / 2;
				CreateObject(startPoint);
			}

			for (int i = 1; i <= count; i++)
			{
				CreateObject(startPoint + dir * _intervalDistance * i);
			}

			if (_placeAtEnd)
				CreateObject(s, "end");
		}

		private void CreateObject(Vector3 position, string name = "step")
		{
			var go = Instantiate(_prefab);
			go.name = name;
			go.transform.SetParent(_parent, false);
			go.transform.localPosition = position + Vector3.up * 1.5f;
		}
	}
}
