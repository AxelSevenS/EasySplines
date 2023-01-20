using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasySplines {
    
    [System.Serializable]
    public class BezierQuadratic : Curve {

        public Vector3 handle = Vector3.forward * 25f;



        public BezierQuadratic(){;}
        
        public BezierQuadratic(ControlPoint cp1Pos, ControlPoint cp2Pos, Vector3 hPos) : base(cp1Pos, cp2Pos) {
            handle = hPos;
        }
        public BezierQuadratic(Vector3 cp1Pos, Vector3 cp2Pos, Vector3 hPos) : base(cp1Pos, cp2Pos) {
            handle = hPos;
        }
        public BezierQuadratic(Transform cp1Pos, Transform cp2Pos, Transform hPos) : base(cp1Pos, cp2Pos) {
            handle = hPos.position;
        }



        public override void Move(Vector3 direction){
            controlPoint1.position += direction;
            controlPoint2.position += direction;
            handle += direction;
        }

        public override OrientedPoint GetPoint(float t){
            Vector3 a = Vector3.Lerp(controlPoint1.position, handle, t);
            Vector3 b = Vector3.Lerp(handle, controlPoint2.position, t);

            Vector3 c = Vector3.Lerp(a, b, t);

            Vector3 tForward = (b - a).normalized;
            float upAngle = Mathf.Lerp(controlPoint1.upAngle, controlPoint2.upAngle, t); 
            Vector3 tUp = Quaternion.AngleAxis(upAngle, tForward) * Vector3.up;
            Quaternion rotation = Quaternion.LookRotation( tForward, tUp );

            return new OrientedPoint( c, rotation );
        }

        public override Vector3 GetTangent(float t){
            Vector3 a = Vector3.Lerp(controlPoint1.position, handle, t);
            Vector3 b = Vector3.Lerp(handle, controlPoint2.position, t);

            return (b - a).normalized;
        }

        public override void UpdateNextSegment(Segment nextSegment) {
            if (nextSegment == null || !(nextSegment is BezierQuadratic nextQuadratic))
                return;

            nextQuadratic.controlPoint1.Set( this.controlPoint2 );
            nextQuadratic.handle = (nextQuadratic.controlPoint1.position * 2) - this.handle;
        }

        public override void UpdatePreviousSegment(Segment previousSegment) {
            if (previousSegment == null || !(previousSegment is BezierQuadratic previousQuadratic))
                return;

            previousQuadratic.controlPoint2.Set( this.controlPoint1 );
            previousQuadratic.handle = (previousQuadratic.controlPoint2.position * 2) - this.handle;
        }

        // public override void UpdateNextSegment( BezierQuadratic nextSegment ){}
        // public override void UpdatePreviousSegment( BezierQuadratic previousSegment ){}
    }
}
