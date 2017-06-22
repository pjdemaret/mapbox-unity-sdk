namespace Mapbox.Unity.MeshGeneration.Interfaces
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Mapbox.VectorTile;
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Filters;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.MeshGeneration.Modifiers;
	using Mapbox.Unity.Utilities;

	[CreateAssetMenu(menuName = "Mapbox/Layer Visualizer/Test Layer Visualizer")]
	public class TestLayerVisualizer : LayerVisualizerBase
	{
		private bool _subdivideLongEdges = true;
		private int _maxEdgeSectionCount = 40;
		private int _preferredEdgeSectionLength = 10;

		[SerializeField]
		private string _classificationKey;
		[SerializeField]
		private string _key;
		public override string Key
		{
			get { return _key; }
			set { _key = value; }
		}

		[SerializeField]
		private List<FilterBase> Filters;

		[SerializeField]
		private ModifierStackBase _defaultStack;
		[SerializeField]
		private List<TypeVisualizerTuple> Stacks;

		private GameObject _container;

		/// <summary>
		/// Creates an object for each layer, extract and filter in/out the features and runs Build method on them.
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="tile"></param>
		public override void Create(VectorTileLayer layer, UnityTile tile)
		{
			_container = new GameObject(Key + " Container");
			_container.transform.SetParent(tile.transform, false);

			//testing each feature with filters
			var fc = layer.FeatureCount();
			var filterOut = false;

			var feature = new VectorFeatureUnity(layer.GetFeature(0, 0), tile, layer.Extent);
			var data = "-43.59299;193.9291,-37.77064;193.0334,-30.75396;191.3912,-26.7231;186.7631,-25.67806;184.5238,-22.99083;183.628,-20.45288;184.8223,-18.66138;187.8082,-14.77982;190.6447,-11.34612;191.2419,-8.808173;190.3461,-7.763136;185.7181,-8.509591;167.3553,-9.554628;156.7556,-8.808173;146.9024,-3.881568;136.7506,12.98832;129.2861,15.67556;124.8073,17.01918;117.9399,17.76564;106.4445,17.16847;99.57714,14.77982;92.41117,4.329441;80.1693,-1.045038;74.94411,-1.194329;71.9583,1.492911;70.46539,4.18015;71.65971,7.165971;71.06255,11.19683;67.92744,13.13761;63.00083,15.97414;58.22352,15.97414;52.10258,12.68974;44.33945,7.165971;35.53128,-3.284404;24.18515,-5.971643;18.3628,-7.912427;12.24187,-9.106755;-8.3603,-5.52377;-22.54295,-5.374478;-32.54545,-6.120934;-42.54795,-14.03336;-66.28523,-20.005;-75.83987,-26.27523;-78.22852,-31.94829;-81.21434,-34.33694;-85.84237,-35.68056;-94.65054,-35.82986;-105.8474,-38.5171;-109.4304,-43.29441;-110.6247,-48.66889;-110.1768,-55.08841;-108.236,-59.56714;-104.5037,-64.34445;-99.72643,-76.73561;-71.80901,-81.06505;-64.79233,-88.38032;-60.91076,-94.94912;-60.46288,-103.4587;-61.35863,-135.8549;-62.10509,-144.8123;-53.29691,-151.3811;-51.05754,-163.7723;-51.35613,-184.8223;-42.69725,-200.9458;-33.4412,-205.2752;-29.26105,-207.3653;-22.39366,-209.754;-17.91493,-214.5313;-10.74896,-220.8015;-4.777314,-227.6689;3.135113,-227.5196;10.30108,-225.8774;18.95997,-225.7281;28.81318,-226.6238;37.91993,-226.7731;55.08841,-228.8632;74.64554,-226.4745;92.26188,-223.9366;108.3853,-220.2043;116.1485,-214.6806;121.2243,-207.8132;120.7765,-199.005;115.9992,-196.3178;111.819,-195.7206;107.9374,-196.3178;104.0559,-199.1543;99.72643,-203.7823;98.6814,-208.1118;98.08424,-211.6947;94.35196,-213.9341;89.42535,-215.7256;81.36363,-216.1735;73.74979,-208.1118;17.46705,-205.7231;14.48123,-201.2444;12.68974,-195.1234;14.92911,-186.4646;23.58799,-178.4028;25.23019,-169.1468;24.48374,-161.5329;20.60217,-153.3219;12.54045,-139.7364;-1.791493,-128.3903;-9.554628,-120.03;-12.24187,-81.9608;-17.16847,-74.79482;-15.0784,-69.86822;-10.0025,-67.77814;-4.030859,-67.03169;1.791493,-71.80901;93.90408,-71.51042;99.87572,-67.92744;105.3995,-61.9558;105.3995,-56.28273;108.236,-54.49124;111.6697,-53.74479;115.5513,-51.65471;118.2385,-48.81818;119.1343,-43.4437;118.5371,-39.86072;117.0442,-38.5171;115.402,-35.68056;115.2527,-31.20183;116.8949,-28.3653;118.2385,-26.42452;115.402,-23.73728;113.7598,-20.45288;116.2977,-18.95997;121.0751,-18.95997;125.7031,-20.005;128.9875,-45.53378;169.5947,-46.28023;180.045,-47.62385;189.5997,-46.28023;193.1826,-43.59299;193.9291?217.0692;286.7881,225.8774;281.7122,228.5646;272.9041,220.5029;259.1693,210.9483;256.4821,203.0359;261.558,200.4979;269.9182,209.1568;284.6981,217.0692;286.7881?162.578;305.7481,157.95;297.6864,157.95;292.0133,150.3361;280.07,141.2294;278.8757,131.824;279.025,132.1226;281.7122,136.3027;283.3545,140.6322;290.2218,139.4379;300.8215,139.7364;305.7481,147.3503;305.7481,148.8432;296.9399,145.5588;289.3261,147.0517;288.1318,150.3361;290.819,151.9783;295.7456,149.7389;305.7481,162.578;305.7481";
			feature.Points.Clear();
			var subs = data.Split('?');
			foreach (var sub in subs)
			{
				var nl = new List<Vector3>();
				var vs = sub.Split(',');
				for (int i = 0; i < vs.Length; i++)
				{
					var vv = vs[i].Split(';');
					nl.Add(new Vector3(float.Parse(vv[0]), 0, float.Parse(vv[1])));
				}
				feature.Points.Add(nl);
			}

			Build(feature, tile, _container);


			var mergedStack = _defaultStack as MergedModifierStack;
			if (mergedStack != null)
			{
				mergedStack.End(tile, _container);
			}

			for (int i = 0; i < Stacks.Count; i++)
			{
				mergedStack = Stacks[i].Stack as MergedModifierStack;
				if (mergedStack != null)
				{
					mergedStack.End(tile, _container);
				}
			}
		}

		/// <summary>
		/// Preprocess features, finds the relevant modifier stack and passes the feature to that stack
		/// </summary>
		/// <param name="feature"></param>
		/// <param name="tile"></param>
		/// <param name="parent"></param>
		private bool IsFeatureValid(VectorFeatureUnity feature)
		{
			if (feature.Properties.ContainsKey("extrude") && !bool.Parse(feature.Properties["extrude"].ToString()))
				return false;

			if (feature.Points.Count < 1)
				return false;

			return true;
		}

		private void Build(VectorFeatureUnity feature, UnityTile tile, GameObject parent)
		{
			if (!IsFeatureValid(feature))
				return;

			//this will be improved in next version and will probably be replaced by filters
			var styleSelectorKey = FindSelectorKey(feature);

			var meshData = new MeshData();
			meshData.TileRect = tile.Rect;

			//and finally, running the modifier stack on the feature
			var mod = Stacks.FirstOrDefault(x => x.Type.Contains(styleSelectorKey));
			GameObject go;
			if (mod != null)
			{
				go = mod.Stack.Execute(tile, feature, meshData, parent, mod.Type);
			}
			else
			{
				if (_defaultStack != null)
					go = _defaultStack.Execute(tile, feature, meshData, parent, _key);
			}
			//go.layer = LayerMask.NameToLayer(_key);
		}

		private string FindSelectorKey(VectorFeatureUnity feature)
		{
			if (string.IsNullOrEmpty(_classificationKey))
			{
				if (feature.Properties.ContainsKey("type"))
				{
					return feature.Properties["type"].ToString().ToLowerInvariant();
				}
				else if (feature.Properties.ContainsKey("class"))
				{
					return feature.Properties["class"].ToString().ToLowerInvariant();
				}
			}
			else if (feature.Properties.ContainsKey(_classificationKey))
			{
				if (feature.Properties.ContainsKey(_classificationKey))
				{
					return feature.Properties[_classificationKey].ToString().ToLowerInvariant();
				}
			}

			return "";
		}
	}
}
