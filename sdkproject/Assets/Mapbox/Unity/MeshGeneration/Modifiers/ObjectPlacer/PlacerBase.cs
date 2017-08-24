using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.MeshGeneration.Data;

public class PlacerBase : ScriptableObject
{
	public virtual void Run(List<Vector3> list, List<Quaternion> rot, VectorFeatureUnity feature, Vector3 point, Vector3 dir)
	{

	}
}
