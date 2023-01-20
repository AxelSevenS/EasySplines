using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace EasySplines {

    [System.Serializable]
    public struct OrientedPoint{

        public Vector3 position;
        public Quaternion rotation;

        

        public static OrientedPoint operator +(OrientedPoint a, Vector3 b) => new OrientedPoint(a.position + b, a.rotation);
        public static OrientedPoint operator +(Vector3 a, OrientedPoint b) => new OrientedPoint(b.position + a, b.rotation);
        public static OrientedPoint operator -(OrientedPoint a, Vector3 b) => new OrientedPoint(a.position - b, a.rotation);
        public static OrientedPoint operator -(Vector3 a, OrientedPoint b) => new OrientedPoint(b.position - a, b.rotation);
        
        public static bool operator ==(OrientedPoint obj1, OrientedPoint obj2) => obj1.Equals(obj2);
        public static bool operator !=(OrientedPoint obj1, OrientedPoint obj2) => !obj1.Equals(obj2);



        public OrientedPoint(OrientedPoint op){
            this.position = op.position;
            this.rotation = op.rotation.normalized;
        }
        public OrientedPoint(Vector3 pos, Quaternion rot){
            this.position = pos;
            this.rotation = rot.normalized;
        }
        public OrientedPoint(Vector3 pos){
            this.position = pos;
            this.rotation = Quaternion.identity;
        }
        public OrientedPoint(Transform obj){
            this.position = obj.position;
            this.rotation = obj.rotation.normalized;
        }

        public void Set(Vector3 pos){
            this.position = pos;
        }
        public void Set(Quaternion rot){
            this.rotation = rot.normalized;
        }
        public void Set(Vector3 pos, Quaternion rot){
            this.position = pos;
            this.rotation = rot.normalized;
        }
        public void Set(OrientedPoint op){
            this.position = op.position;
            this.rotation = op.rotation;
        }

        public override bool Equals(/* [NotNullWhen(true)]  */object obj) {
            if (obj is OrientedPoint op)
                return position == op.position && rotation == op.rotation;
            else
                return false;
        }

        public override int GetHashCode() => System.Tuple.Create(position, rotation).GetHashCode();
    }
}
