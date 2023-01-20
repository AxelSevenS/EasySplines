using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasySplines {
    
    [System.Serializable]
    public class LineSegment : Segment {

        public LineSegment(){;}

        public LineSegment(Vector3 cp1Pos, Vector3 cp2Pos) : base(cp1Pos, cp2Pos) {;}
        public LineSegment(ControlPoint cp1Pos, ControlPoint cp2Pos) : base(cp1Pos, cp2Pos){;}
        public LineSegment(Transform cp1Pos, Transform cp2Pos) : base(cp1Pos, cp2Pos){;}
        

        public override void Move(Vector3 direction){
            controlPoint1.position += direction;
            controlPoint2.position += direction;
        }

        public override OrientedPoint GetPoint(float t){
            Vector3 position = Vector3.Lerp(controlPoint1.position, controlPoint2.position, t);
            Vector3 tForward = (controlPoint2.position - controlPoint1.position).normalized;
            float upAngle = Mathf.Lerp(controlPoint1.upAngle, controlPoint2.upAngle, t);
            Vector3 tUp = Quaternion.AngleAxis(upAngle, tForward) * Vector3.up;
            Quaternion rotation = Quaternion.LookRotation( tForward, tUp );

            return new OrientedPoint( position , rotation );
        }

        public override Vector3 GetTangent(float t){
            return (controlPoint2.position - controlPoint1.position).normalized;
        }

        public override float GetUniformT(float t) => t;


        public override void UpdateNextSegment(Segment nextSegment) {
            if (nextSegment == null || !(nextSegment is LineSegment))
                return;

            nextSegment.controlPoint1.Set( this.controlPoint2 );
        }

        public override void UpdatePreviousSegment(Segment previousSegment) {
            if (previousSegment == null || !(previousSegment is LineSegment))
                return;

            previousSegment.controlPoint2.Set( this.controlPoint1 );
        }

    }
}
