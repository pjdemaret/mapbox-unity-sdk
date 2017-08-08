namespace Mapbox.Unity.Map
{
    using UnityEngine;
    using Mapbox.Map;
    using Mapbox.Unity.Utilities;
    using Mapbox.Utils;
    using System.Collections.Generic;

    public class QuadTreeTileProvider : AbstractTileProvider
    {
        [SerializeField]
        Camera _camera;

        // TODO: change to Vector4 to optimize for different aspect ratios.
        [SerializeField]
        int _visibleBuffer;

        [SerializeField]
        int _disposeBuffer;

        [SerializeField]
        float _updateInterval;

        [SerializeField]
        float _zoomSpeed = 10.0f;

        Plane _groundPlane;
        Ray _rayNE;
        Ray _raySW;
        float _hitDistanceNE;
        float _hitDistanceSW;
        Vector3 _viewportSW;
        Vector3 _viewportNE;
        float _elapsedTime;
        bool _shouldUpdate;
        int _previousZoomLevel; 
        Vector2d _currentLatitudeLongitude;
        HashSet<UnwrappedTileId> _cachedTiles;
        HashSet<UnwrappedTileId> _currentTiles;

        float _zoomSwitchDistance = 10.0f;



        internal override void OnInitialized()
        {
            _groundPlane = new Plane(Vector3.up, Mapbox.Unity.Constants.Math.Vector3Zero);
            _viewportSW = new Vector3(0.0f, 0.0f, 0);
            _viewportNE = new Vector3(1.0f, 1.0f, 0);
            _shouldUpdate = true;
            _currentTiles = new HashSet<UnwrappedTileId>();
            _cachedTiles = new HashSet<UnwrappedTileId>();
            _previousZoomLevel = _map.Zoom;

            var currentPosition = _camera.transform.position;
            _zoomSwitchDistance = (currentPosition.y - 10.0f /*buffer - so we don't landup inside the ground*/) / (22.0f - _map.Zoom);
            
        }

        public static float MapScaleToZoomLevel(float mapScale, float latitude, float ppi)
        {
            const float MetersPerInch = 2.54f / 100;

            const double EarthRadius = 6371000.00;
            const double EarthCircumference = EarthRadius * System.Math.PI * 2;
            double realLengthInMeters = EarthCircumference * System.Math.Cos(latitude* System.Math.PI /180.0);

            double zoomLevelExp = (realLengthInMeters * ppi) / (256 * MetersPerInch * mapScale);

            return (float)System.Math.Log(zoomLevelExp, 2);
        }

        void Update()
        {
            if (!_shouldUpdate)
            {
                return;
            }

            _elapsedTime += Time.deltaTime;
            if (_elapsedTime >= _updateInterval)
            {
                _elapsedTime = 0f;

                if (_previousZoomLevel != _map.Zoom)
                {
                    var currentPosition = _camera.transform.position;
                    currentPosition.y += (_previousZoomLevel - _map.Zoom) * _zoomSpeed * _zoomSwitchDistance;
                    _camera.transform.position = currentPosition;
                    _previousZoomLevel = _map.Zoom;
                }
                _rayNE = _camera.ViewportPointToRay(_viewportNE);
                _raySW = _camera.ViewportPointToRay(_viewportSW);
                if (_groundPlane.Raycast(_rayNE, out _hitDistanceNE) && _groundPlane.Raycast(_raySW, out _hitDistanceSW))
                {
                    var northEast = _rayNE.GetPoint(_hitDistanceNE).GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);                  
                    //var startTile = TileCover.CoordinateToTileId(northEast, _map.Zoom);
                
                    var southWest = _raySW.GetPoint(_hitDistanceSW).GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
                    //var endTile = TileCover.CoordinateToTileId(southWest, _map.Zoom);

                    Vector2dBounds tileBounds = new Vector2dBounds(southWest, northEast);

                    var tilesToRequest = TileCover.Get(tileBounds, _map.Zoom);

                    _cachedTiles.Clear();
                    foreach(var tile in _currentTiles)
                    {
                        _cachedTiles.Add(tile);                   
                    }
                    _currentTiles.Clear();

                    foreach(var tileRequest in tilesToRequest)
                    {                       
                        var _currentTile = new UnwrappedTileId(tileRequest.Z,tileRequest.X, tileRequest.Y);
                        if (!_cachedTiles.Contains(_currentTile))
                        {
                            AddTile(_currentTile);
                            _currentTiles.Add(_currentTile);
                        }
                        else
                        {
                            //this tile was cached, so don't destroy it. 
                            _currentTiles.Add(_currentTile);
                            _cachedTiles.Remove(_currentTile);
                        }
                    }
                    Cleanup(_cachedTiles);
                }
            }
        }

        void Cleanup(HashSet<UnwrappedTileId> tilesToDispose)
        {
            foreach(var tile in tilesToDispose)
            {
                RemoveTile(tile);
            } 
        }

    }
}
