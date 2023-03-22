using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SevenGame.Utility;

namespace EasySplines {
    
    public class FollowSpline : MonoBehaviour{

        public MovementDirection movementDirection;

        [SerializeField] private Spline spline;
        private OrientedPoint splinePosition;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float moveSpeed = 0;
        [SerializeField] private float acceleration = 0.5f;
        [Range(0, 1f)] [SerializeField] private float t;

        [SerializeField] private float currentVelocity;


        private bool goingForward => movementDirection == MovementDirection.Forward;


        protected virtual async void StopAndTurnBack() {
            MovementDirection oldDirection = movementDirection;
            movementDirection = MovementDirection.None;

            await System.Threading.Tasks.Task.Delay(3000);

            movementDirection = (MovementDirection)(-(int)oldDirection);
        }

        protected virtual async void StopAndContinue() {
            MovementDirection oldDirection = movementDirection;
            movementDirection = MovementDirection.None;

            await System.Threading.Tasks.Task.Delay(3000);

            movementDirection = oldDirection;
        }

        private void Update(){
            if (movementDirection == MovementDirection.None) {
                return;
            }


            float direction = (float)movementDirection;

            bool stoppingPointForward = spline.hasStoppingPoint && ((spline.stoppingPoint < t && movementDirection == MovementDirection.Backward) || (spline.stoppingPoint > t && movementDirection == MovementDirection.Forward));
            bool endOfTheLine = goingForward ? spline.nextSpline == null : spline.previousSpline == null;

            if ( stoppingPointForward || endOfTheLine ) {

                // When at the end of the line or at the stopping point, slow down
                float stoppingPoint = stoppingPointForward ? spline.stoppingPoint : (goingForward ? 1f : 0f);

                // t = Mathf.SmoothStep(t, stoppingPoint, moveSpeed * GameUtility.timeDelta);
                moveSpeed = Mathf.SmoothStep(moveSpeed, Mathf.Abs(t - stoppingPoint), moveSpeed * GameUtility.timeDelta);


                bool reachedStoppingPoint = Mathf.Abs(t - stoppingPoint) < 0.01f;

                if (reachedStoppingPoint) {
                    t = stoppingPoint;
                    if (stoppingPointForward) {
                        // If at the stopping point, start moving again
                        StopAndContinue();
                    } else {
                        // If at the end of the line, stop and turn around
                        StopAndTurnBack();
                    }
                }

            } else {
                // If not at the end of the line or at the stopping point, move at the set speed
                moveSpeed = Mathf.SmoothStep(moveSpeed, maxSpeed, acceleration * GameUtility.timeDelta);
            }

            // Move along spline
            float distanceToMove = (moveSpeed * direction) / 40f * GameUtility.timeDelta;
            t += distanceToMove;


            // If the object has reached the end of the spline, go to the next one
            while ( goingForward && t > 1f && spline.nextSpline != null) {
                spline = spline.nextSpline;
                t -= 1f;
            }
            while ( !goingForward && t < 0f && spline.previousSpline != null) {
                spline = spline.previousSpline;
                t += 1f;
            }
 
            
            // Move
            splinePosition = spline.GetPointUniform(t);

            transform.position = Vector3.Lerp(transform.position, splinePosition.position, 5f * GameUtility.timeDelta);
            transform.rotation = Quaternion.Slerp(transform.rotation, splinePosition.rotation, 5f * GameUtility.timeDelta);
        }



        public enum MovementDirection {
            Backward = -1,
            None = 0,
            Forward = 1
        }

    }
}