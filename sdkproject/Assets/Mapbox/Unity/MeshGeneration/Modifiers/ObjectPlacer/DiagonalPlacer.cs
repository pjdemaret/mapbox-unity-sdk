using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.MeshGeneration.Data;

[CreateAssetMenu(menuName = "Mapbox/Modifiers/Placer/Diagonal Placer")]
public class DiagonalPlacer : PlacerBase
{
	[SerializeField]
	private float _lineWidth;
	[SerializeField]
	[Tooltip("Multiplier for line width")]
	private float _distanceMultiplier;
	[SerializeField]
	[Tooltip("Fixed value to add after the multiplication")]
	private float _additionalDistance;
	[SerializeField]
	private float _minRotationAngle;
	[SerializeField]
	private float _maxRotationAngle;

	public override void Run(List<Vector3> list, List<Quaternion> rot, VectorFeatureUnity feature, Vector3 point, Vector3 dir)
	{
		dir = new Vector3(-dir.z, dir.y, dir.x);
		var rnd = Random.Range(_minRotationAngle, _maxRotationAngle);
		rot.Add(Quaternion.FromToRotation(Vector3.forward, Quaternion.Euler(0, rnd, 0) * (dir * -1)));
		dir.Normalize();
		dir = point + ((dir * _lineWidth) * _distanceMultiplier) + (dir * _additionalDistance);
		list.Add(dir);
	}
}
