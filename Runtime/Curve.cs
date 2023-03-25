using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasySplines {

    [System.Serializable]
    public abstract class Curve : Segment {

        private const int LENGTH_PRECISION = 100;


        
        [SerializeField] protected float[] _arcLengths;




        public float[] arcLengths {
            get {
                if (_arcLengths == null || _arcLengths.Length == 0)
                    UpdateLength();
                return _arcLengths;
            }
            private set {
                _arcLengths = value;
            }
        }


        public Curve() {;}
        public Curve(ControlPoint cp1, ControlPoint cp2) : base(cp1, cp2) {}
        public Curve(Vector3 cp1Pos, Vector3 cp2Pos) : base(cp1Pos, cp2Pos) {}
        public Curve(Transform cp1, Transform cp2) : base(cp1, cp2) {}


        public override float GetUniformT(float t) {
            // Get the distance along the curve the T value represents
            float distance = t * length;

            // Find the two arc lengths that the distance lies between
            int upperBound = 1;
            while (upperBound < LENGTH_PRECISION && arcLengths[upperBound] < distance) {
                upperBound++;
            }
            int lowerBound = upperBound - 1;

            // Lerp between them
            float tBetweenBounds = Mathf.InverseLerp(_arcLengths[lowerBound], _arcLengths[upperBound], distance);

            // Get the point on the curve between the two bounds, convert it into a Curve-wise T value.
            const float PERCENTILE = 1f / (float)LENGTH_PRECISION;
            float uniformT = Mathf.Lerp(lowerBound * PERCENTILE, upperBound * PERCENTILE, tBetweenBounds);

            return uniformT;
        }


        
        public override void UpdateLength() {
            _arcLengths = new float[LENGTH_PRECISION + 1];
            _arcLengths[0] = 0;

            Vector3 lastPoint = GetPoint(0f).position;
            _length = 0;

            for (int i = 0; i < LENGTH_PRECISION + 1; i++) {
                float t = (float)i / LENGTH_PRECISION;
                Vector3 currentPoint = GetPoint(t).position;

                _length += (lastPoint - currentPoint).magnitude;
                _arcLengths[i] = _length;

                lastPoint = currentPoint;
            }
        }

        // public float SampleArcLengths(float t){
            // int count = arcLengths.Length; 
            // int precision = count - 1;
            // float lengthIndex = t * (precision);
            // int lowerBound = Mathf.FloorToInt(lengthIndex);
            // int upperBound = Mathf.FloorToInt(lengthIndex + 1);
            
            // if ( upperBound >= count )
                // return arcLengths[precision];
            // if ( lowerBound < 0 )
                // return arcLengths[0];
            // return Mathf.Lerp( arcLengths[lowerBound], arcLengths[upperBound], lengthIndex - lowerBound);
        // }
    }

}
