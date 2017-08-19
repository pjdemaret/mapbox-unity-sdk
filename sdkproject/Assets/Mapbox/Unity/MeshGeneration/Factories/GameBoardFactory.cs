namespace Mapbox.Unity.MeshGeneration.Factories
{
    using System.Collections.Generic;
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Enums;
    using Mapbox.Unity.MeshGeneration.Data;
    using Mapbox.Unity.MeshGeneration.Interfaces;
    using Mapbox.Map;

    /// <summary>
    /// Uses vector tile api to visualize vector data.
    /// Fetches the vector data for given tile and passes layer data to layer visualizers.
    /// </summary>
    [CreateAssetMenu(menuName = "Mapbox/Factories/Game Board Factory")]
    public class GameBoardFactory : AbstractTileFactory
    {
        [SerializeField]
        private string _mapId = "";

        [SerializeField]
        public GameObject PoiPrefab;

        public List<LayerVisualizerBase> Visualizers;

        private Dictionary<string, List<LayerVisualizerBase>> _layerBuilder;
        private Dictionary<UnityTile, VectorTile> _cachedData = new Dictionary<UnityTile, VectorTile>();
        private GameObject _container;
        public void OnEnable()
        {
            if (Visualizers == null)
            {
                Visualizers = new List<LayerVisualizerBase>();
            }
        }

        /// <summary>
        /// Sets up the Mesh Factory
        /// </summary>
        /// <param name="fs"></param>
        internal override void OnInitialized()
        {
            _layerBuilder = new Dictionary<string, List<LayerVisualizerBase>>();
            foreach (LayerVisualizerBase factory in Visualizers)
            {
                if (_layerBuilder.ContainsKey(factory.Key))
                {
                    _layerBuilder[factory.Key].Add(factory);
                }
                else
                {
                    _layerBuilder.Add(factory.Key, new List<LayerVisualizerBase>() { factory });
                }
            }
        }

        internal override void OnRegistered(UnityTile tile)
        {
            var vectorTile = new VectorTile();
            tile.AddTile(vectorTile);

            Progress++;
            vectorTile.Initialize(_fileSource, tile.CanonicalTileId, _mapId, () =>
            {
                if (vectorTile.HasError)
                {
                    tile.VectorDataState = TilePropertyState.Error;
                    Progress--;
                    return;
                }

                _cachedData.Add(tile, vectorTile);

                // FIXME: we can make the request BEFORE getting a response from these!
                if (tile.HeightDataState == TilePropertyState.Loading ||
                    tile.RasterDataState == TilePropertyState.Loading)
                {
                    tile.OnHeightDataChanged += DataChangedHandler;
                    tile.OnRasterDataChanged += DataChangedHandler;
                }
                else
                {
                    PlaceMines(tile);
                }
            });
        }

        internal override void OnUnregistered(UnityTile tile)
        {
            // We are no longer interested in this tile's notifications.
            tile.OnHeightDataChanged -= DataChangedHandler;
            tile.OnRasterDataChanged -= DataChangedHandler;
        }

        private void DataChangedHandler(UnityTile t)
        {
            if (t.RasterDataState != TilePropertyState.Loading &&
                t.HeightDataState != TilePropertyState.Loading)
            {
                PlaceMines(t);
            }
        }

        /// <summary>
        /// Fetches the vector data and passes each layer to relevant layer visualizers
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="e"></param>
        private void PlaceMines(UnityTile tile)
        {
            _container = new GameObject("Mines Container");
            _container.transform.SetParent(tile.transform, false);

            tile.OnHeightDataChanged -= DataChangedHandler;
            tile.OnRasterDataChanged -= DataChangedHandler;

            tile.VectorDataState = TilePropertyState.Loading;

            // TODO: move unitytile state registrations to layer visualizers. Not everyone is interested in this data
            // and we should not wait for it here!

                if (_layerBuilder.ContainsKey("building") && _layerBuilder.ContainsKey("poi_label"))
                {
                        //my stuff begins------------------------------------------------------------------------
                        
                // get the building layer
                        var layer =  _cachedData[tile].Data.GetLayer("building");
                        List<VectorFeatureUnity> buildingFeatures = new List<VectorFeatureUnity>();

                //load all features in the buildingFeaturesList
                        var fc = layer.FeatureCount();
                        for (int i = 0; i < fc; i++)
                        {
                            var feature = new VectorFeatureUnity(layer.GetFeature(i, 0), tile, layer.Extent);
                            buildingFeatures.Add(feature);
                        }


                // get the poi_label layer
                var poilayer = _cachedData[tile].Data.GetLayer("poi_label");
                var pc = poilayer.FeatureCount();
                for (int i = 0; i < pc; i++)
                {
                    var feature = new VectorFeatureUnity(poilayer.GetFeature(i, 0), tile, poilayer.Extent);

                    foreach(var bldgFeature in buildingFeatures)
                    {
                        if (bldgFeature.Points == null || feature.Points == null || bldgFeature.Points.Count==0 || feature.Points.Count == 0 || bldgFeature.Points[0].Count == 0 || feature.Points[0].Count == 0)
                            return;
                        if (GetRect(bldgFeature.Points[0]).Contains(feature.Points[0][0]))
                            DoMinePlacement(feature, tile, _container);
                    }
                }



                //my stuff ends------------------------------------------------------------------------

            }


            tile.VectorDataState = TilePropertyState.Loaded;
            Progress--;

            _cachedData.Remove(tile);
        }

        void DoMinePlacement(VectorFeatureUnity feature, UnityTile tile, GameObject parent)
        {

            int selpos = feature.Points[0].Count / 2;
            var met = feature.Points[0][selpos];

            var go = Instantiate(PoiPrefab);
            var rx = (met.x - tile.Rect.Min.x) / tile.Rect.Size.x;
            var ry = 1 - (met.z - tile.Rect.Min.y) / tile.Rect.Size.y;
            var h = tile.QueryHeightData((int)rx, (int)ry);
            met.y += h;
            go.transform.position = met;
            go.transform.SetParent(parent.transform, false);
            /*
            if (!_scaleDownWithWorld)
            {
                go.transform.localScale = Vector3.one / go.transform.lossyScale.x;
            }
            */
        }

        Rect GetRect(List<Vector3> points)
        {
            Vector3 max = points[0];
            Vector3 min = points[0];

            for (int i = 1; i < points.Count; i++)
            {
                max = Vector3.Max(max, points[i]);
                min = Vector3.Min(min, points[i]);
            }

            return new Rect(new Vector2(min.x,min.y),new Vector2(Mathf.Abs(max.x-min.x),Mathf.Abs(max.y-min.y)));
        }
    }
}
