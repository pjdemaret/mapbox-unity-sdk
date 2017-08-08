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
        Ray _rayTopLeft;
        Ray _rayBottomRight;
        float _hitDistanceTopLeft;
        float _hitDistanceBottomRight;
        Vector3 _viewportTopLeft;
        Vector3 _viewportBottomRight;
        float _elapsedTime;
        bool _shouldUpdate;
        int _previousZoomLevel; 
        Vector2d _currentLatitudeLongitude;
        HashSet<UnwrappedTileId> _cachedTiles;
        HashSet<UnwrappedTileId> _currentTiles;



        internal override void OnInitialized()
        {
            _groundPlane = new Plane(Vector3.up, Mapbox.Unity.Constants.Math.Vector3Zero);
            _viewportTopLeft = new Vector3(0.0f, 1.0f, 0);
            _viewportBottomRight = new Vector3(1.0f, 0.0f, 0);
            _shouldUpdate = true;
            _currentTiles = new HashSet<UnwrappedTileId>();
            _cachedTiles = new HashSet<UnwrappedTileId>();
            _previousZoomLevel = _map.Zoom;
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

                if(_previousZoomLevel != _map.Zoom)
                {
                    var currentPosition = _camera.transform.position;
                    currentPosition.y += (_previousZoomLevel - _map.Zoom) * _zoomSpeed;
                    _camera.transform.position = currentPosition;
                    _previousZoomLevel = _map.Zoom;
                }
                _rayTopLeft = _camera.ViewportPointToRay(_viewportTopLeft);
                _rayBottomRight = _camera.ViewportPointToRay(_viewportBottomRight);
                if (_groundPlane.Raycast(_rayTopLeft, out _hitDistanceTopLeft) && _groundPlane.Raycast(_rayBottomRight, out _hitDistanceBottomRight))
                {
                    _currentLatitudeLongitude = _rayTopLeft.GetPoint(_hitDistanceTopLeft).GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);                  
                    var startTile = TileCover.CoordinateToTileId(_currentLatitudeLongitude, _map.Zoom);
                
                    _currentLatitudeLongitude = _rayBottomRight.GetPoint(_hitDistanceBottomRight).GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
                    var endTile = TileCover.CoordinateToTileId(_currentLatitudeLongitude, _map.Zoom);

                    _cachedTiles.Clear();
                    foreach(var tile in _currentTiles)
                    {
                        _cachedTiles.Add(tile);                   
                    }
                    _currentTiles.Clear();
                    for (int x = System.Math.Max(startTile.X,0); x <= endTile.X; x++)
                    {
                        for (int y = System.Math.Max(startTile.Y,0) ; y <= endTile.Y; y++)
                        {
                            var _currentTile = new UnwrappedTileId(_map.Zoom, x, y);
                            if(!_cachedTiles.Contains(_currentTile))
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
