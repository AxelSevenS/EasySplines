using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace EasySplines {

    [System.Serializable]
    public struct ControlPoint {
        
        public Vector3 position;
        public float upAngle;

        

        public static ControlPoint operator +(ControlPoint a, Vector3 b) => new ControlPoint(a.position + b, a.upAngle);
        public static ControlPoint operator +(Vector3 a, ControlPoint b) => new ControlPoint(b.position + a, b.upAngle);
        public static ControlPoint operator -(ControlPoint a, Vector3 b) => new ControlPoint(a.position - b, a.upAngle);
        public static ControlPoint operator -(Vector3 a, ControlPoint b) => new ControlPoint(b.position - a, b.upAngle);
        
        
        public static bool operator ==(ControlPoint obj1, ControlPoint obj2) => obj1.Equals(obj2);
        public static bool operator !=(ControlPoint obj1, ControlPoint obj2) => !obj1.Equals(obj2);



        public ControlPoint(ControlPoint op){
            this.position = op.position;
            this.upAngle = op.upAngle;
        }
        public ControlPoint(Vector3 pos, float angle){
            this.position = pos;
            this.upAngle = angle;
        }
        public ControlPoint(Vector3 pos){
            this.position = pos;
            this.upAngle = 0f;
        }

        public void SetPosition(Vector3 pos){
            this.position = pos;
        }
        public void Set(float angle){
            this.upAngle = angle;
        }
        public void Set(Vector3 pos, float rot){
            this.position = pos;
            this.upAngle = rot;
        }
        public void Set(ControlPoint op){
            this.position = op.position;
            this.upAngle = op.upAngle;
        }

        public override bool Equals(object obj) {
            if (obj is ControlPoint op)
                return position == op.position && upAngle == op.upAngle;
            else
                return false;
        }

        public override int GetHashCode() => System.Tuple.Create(position, upAngle).GetHashCode();
    }
}
