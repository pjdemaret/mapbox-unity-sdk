namespace Mapbox.Unity.MeshGeneration.Modifiers
{
    using System.Collections.Generic;
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Components;

    public enum DecorationType
    {
        SegmentRoadCenter,
        RegularInterVal
    }

    /// <summary>
    /// Decoration modifier simply adds some decoration(ex: lamps) all along the roads
    /// </summary>
    [CreateAssetMenu(menuName = "Mapbox/Modifiers/Decoration Modifier")]
    public class DecorationModifier : GameObjectModifier
    {
        [SerializeField]
        private GameObject _prefab;

        [SerializeField]
        private DecorationType _type;

        [SerializeField]
        private float _distanceBetweenObject;

        [SerializeField]
        private float _translateY;

        [SerializeField]
        private float _scaleFactor;

        //[SerializeField]
       // private float _offset;

        public override void Run(FeatureBehaviour fb)
        {
            if(_prefab == null)
            {
                return;
            }

            foreach (var roadSegment in fb.Data.Points)
            {
                var count = roadSegment.Count;
                for (int i = 1; i < count; i++)
                {
                    // Lets spawn a deco object in the center of each segment
                    Vector3 p1 = roadSegment[i - 1];
                    //DEBUG: Draw extremity segment cube 
                    //GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //cube1.transform.localPosition = p1;
                    Vector3 p2 = roadSegment[i];
                    //DEBUG: Draw extremity segment cube 
                    //GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //cube2.transform.localPosition = p2;

                    if(_type == DecorationType.SegmentRoadCenter)
                    {
                        //Get midpoint of p1 p2
                        Vector3 midPoint = new Vector3( ( p1.x + p2.x )/2, _translateY, ( p1.z + p2.z ) /2);

                        //Instantiate prefab at midpoint
                        var transform = ((GameObject)Instantiate(_prefab)).transform;
                        transform.SetParent(fb.transform, false);
                        transform.localPosition = midPoint;

                        //Apply scale
                        var scale = transform.localScale;
                        scale += new Vector3(_scaleFactor, _scaleFactor, _scaleFactor);
                        transform.localScale = scale;
                    }
                    else if(_type == DecorationType.RegularInterVal)
                    {
                        if(_distanceBetweenObject <= 0)
                        {
                            return;
                        }

                        float distance = Vector3.Distance(p1,p2);

                        int numberToSpawn = (int) (distance /_distanceBetweenObject);

                        for(int j = 1; j < numberToSpawn; j++)
                        {
                            //Instantiate prefab at midpoint
                            var transform = ((GameObject)Instantiate(_prefab)).transform;
                            transform.SetParent(fb.transform, false);

                            Vector3 pointToSpawn = Vector3.Lerp(p1, p2, j/numberToSpawn);
                            transform.localPosition = new Vector3(pointToSpawn.x, _translateY, pointToSpawn.z);

                            //Apply scale
                            var scale = transform.localScale;
                            scale += new Vector3(_scaleFactor, _scaleFactor, _scaleFactor);
                            transform.localScale = scale;
                        }
                    }
                }
            }
        }
    }
}

