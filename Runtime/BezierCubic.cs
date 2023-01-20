using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasySplines {
    
    [System.Serializable]
    public class BezierCubic : Curve {

        public Vector3 handle1 = Vector3.forward * 50f/3f;
        public Vector3 handle2 = Vector3.forward * 100f/3f;



        public BezierCubic(){;}
        public BezierCubic(ControlPoint cp1, ControlPoint cp2, Vector3 h1Pos, Vector3 h2Pos) : base(cp1, cp2) {
            handle1 = h1Pos;
            handle2 = h2Pos;
        }
        public BezierCubic(Vector3 cp1Pos, Vector3 cp2Pos, Vector3 h1Pos, Vector3 h2Pos) : base(cp1Pos, cp2Pos) {
            handle1 = h1Pos;
            handle2 = h2Pos;
        }
        public BezierCubic(Transform cp1, Transform cp2, Transform h1, Transform h2) : base(cp1, cp2) {
            handle1 = h1.position;
            handle2 = h2.position;
        }
        public BezierCubic(Transform cp1Pos, Transform cp2Pos, Vector3 h1, Vector3 h2) : base(cp1Pos, cp2Pos) {
            handle1 = h1;
            handle2 = h2;
        }




        public override void Move(Vector3 direction){
            controlPoint1.position += direction;
            controlPoint2.position += direction;
            handle1 += direction;
            handle2 += direction;
        } 

        public override OrientedPoint GetPoint(float t){
            Vector3 a = Vector3.Lerp(controlPoint1.position, handle1, t);
            Vector3 b = Vector3.Lerp(handle1, handle2, t);
            Vector3 c = Vector3.Lerp(handle2, controlPoint2.position, t);

            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);
            Vector3 f = Vector3.Lerp( d, e, t );

            Vector3 tForward = (e - d).normalized;
            float upAngle = Mathf.Lerp(controlPoint1.upAngle, controlPoint2.upAngle, t); 
            Vector3 tUp = Quaternion.AngleAxis(upAngle, tForward) * Vector3.up;
            Quaternion rotation = Quaternion.LookRotation( tForward, tUp );

            return new OrientedPoint( f, rotation );
        }

        public override Vector3 GetTangent(float t){
            Vector3 a = Vector3.Lerp(controlPoint1.position, handle1, t);
            Vector3 b = Vector3.Lerp(handle1, handle2, t);
            Vector3 c = Vector3.Lerp(handle2, controlPoint2.position, t);

            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);

            return (e - d).normalized;
        }

        public override void UpdateNextSegment(Segment nextSegment) {
            if (nextSegment == null || !(nextSegment is BezierCubic nextCubic))
                return;
            
            nextCubic.controlPoint1.Set( this.controlPoint2 );
            nextCubic.handle1 = (nextCubic.controlPoint1.position * 2) - this.handle2;
        }

        public override void UpdatePreviousSegment(Segment previousSegment) {
            if (previousSegment == null || !(previousSegment is BezierCubic previousCubic))
                return;
            
            previousCubic.controlPoint2.Set( this.controlPoint1 );
            previousCubic.handle2 = (previousCubic.controlPoint2.position * 2) - this.handle1;
        }

        // public Vector3 GetVelocity(float t){
        //     float tPow2 = Mathf.Pow(t, 2f);
        //     Vector3 p0 = controlPoint1.position * ( (-3f * tPow2) + (6f * t) - 3f );
        //     Vector3 p1 = handle1.position * ( (9f * tPow2) - (12f * t) + 3f );
        //     Vector3 p2 = handle2.position * ( (-9f * tPow2) + (6f * t) );
        //     Vector3 p3 = controlPoint2.position * ( 3f * tPow2 );

        //     return p0 + p1 + p2 + p3;
        // }

        // public Vector3 GetAcceleration(float t){
        //     Vector3 p0 = controlPoint1.position * ( -6f * t + 6f );
        //     Vector3 p1 = handle1.position * ( 18f * t - 12f );
        //     Vector3 p2 = handle2.position * ( -18f * t + 6f );
        //     Vector3 p3 = controlPoint2.position * ( 6f * t );

        //     return p0 + p1 + p2 + p3;
        // }
    }
}
