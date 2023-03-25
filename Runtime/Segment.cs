using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasySplines {
    
    [System.Serializable]
    public abstract class Segment {

        public ControlPoint controlPoint1 = new ControlPoint(Vector3.zero);
        public ControlPoint controlPoint2 = new ControlPoint(Vector3.forward * 50f);

        [SerializeField] protected float _length;



        public float length {
            get {
                if (_length == 0f)
                    UpdateLength();
                return _length;
            }
            private set {
                _length = value;
            }
        }



        public Segment() {;}
        public Segment(ControlPoint cp1, ControlPoint cp2) {
            controlPoint1 = cp1;
            controlPoint2 = cp2;
        }
        public Segment(Vector3 cp1Pos, Vector3 cp2Pos) {
            controlPoint1.position = cp1Pos;
            controlPoint2.position = cp2Pos;
        }
        public Segment(Transform cp1, Transform cp2) {
            controlPoint1.position = cp1.position;
            controlPoint2.position = cp2.position;
        }



        public abstract void Move(Vector3 direction);

        public float GetTOfDistance(float distance) {
            float distanceT = distance / length;
            Debug.Log($"{distance} / {length}");
            return GetUniformT(distanceT);
        }

        public float GetDistanceOfT(float t) {
            return t * length;
        }

        public abstract float GetUniformT(float t);

        public abstract OrientedPoint GetPoint(float t);

        public abstract Vector3 GetTangent(float t);

        public abstract void UpdateNextSegment( Segment nextSegment );
        public abstract void UpdatePreviousSegment( Segment previousSegment );

        public virtual void UpdateLength() {

            _length = 0;

            Vector3 firstPoint = GetPoint(0f).position;
            Vector3 lastPoint = GetPoint(1f).position;
            
            _length = (firstPoint - lastPoint).magnitude;
        }
    }
}
