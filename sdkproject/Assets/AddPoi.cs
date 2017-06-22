using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;

public class AddPoi : MonoBehaviour
{
	public AbstractMap Map;
	public List<string> Coordinates;

	void Start()
	{
		Map.OnInitialized += () =>
		{
			foreach (var item in Coordinates)
			{
				var latLonSplit = item.Split(',');
				var llpos = new Vector2d(double.Parse(latLonSplit[0]), double.Parse(latLonSplit[1]));
				var pos = Conversions.GeoToWorldPosition(llpos, Map.CenterMercator, Map.WorldRelativeScale);
				var gg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				gg.transform.position = new Vector3((float)pos.x, 0, (float)pos.y);
			}
		};
	}
}
