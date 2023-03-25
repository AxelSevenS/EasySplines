using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SevenGame.Utility;

namespace EasySplines {
    
    [DefaultExecutionOrder(-10)]
    public class FollowSpline : MonoBehaviour{

        public MovementDirection movementDirection;

        [SerializeField] private Spline spline;
        private OrientedPoint splinePosition;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float moveSpeed = 0;
        [SerializeField] private float acceleration = 0.5f;
        [SerializeField] private float travelledDistance;

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

        private void MoveToPoint(float distance) {
            splinePosition = spline.GetPointWithDistance(distance);

            // transform.position = Vector3.Lerp(transform.position, splinePosition.position, 5f * GameUtility.timeDelta);
            // transform.rotation = Quaternion.Slerp(transform.rotation, splinePosition.rotation, 5f * GameUtility.timeDelta);
            transform.position = splinePosition.position;
            transform.rotation = splinePosition.rotation;
        }

        private void Update() {
            if (movementDirection == MovementDirection.None) {
                return;
            }


            float direction = (float)movementDirection;

            bool stoppingPointForward = spline.hasStoppingPoint && ((spline.stoppingPoint < spline.segment.GetTOfDistance(travelledDistance) && movementDirection == MovementDirection.Backward) || (spline.stoppingPoint > spline.segment.GetTOfDistance(travelledDistance) && movementDirection == MovementDirection.Forward));
            bool endOfTheLine = goingForward ? spline.nextSpline == null : spline.previousSpline == null;

            if (stoppingPointForward || endOfTheLine) {

                // When at the end of the line or at the stopping point, slow down
                float stoppingPoint = stoppingPointForward ? spline.segment.GetDistanceOfT(spline.stoppingPoint) : (goingForward ? spline.length : 0f);

                // t = Mathf.SmoothStep(t, stoppingPoint, moveSpeed * GameUtility.timeDelta);
                float distanceToStoppingPoint = Mathf.Abs(travelledDistance - stoppingPoint);
                float stoppingDistance = goingForward ? stoppingPoint - spline.length : spline.length - stoppingPoint;
                float slowDownCoefficient = 1 - (stoppingDistance / distanceToStoppingPoint);
                moveSpeed = Mathf.SmoothStep(moveSpeed, Mathf.Min(maxSpeed, distanceToStoppingPoint), slowDownCoefficient * 50f * GameUtility.timeDelta);


                bool reachedStoppingPoint = Mathf.Abs(travelledDistance - stoppingPoint) < 0.01f;

                if (reachedStoppingPoint) {
                    travelledDistance = stoppingPoint;
                    if (stoppingPointForward) {
                        StopAndContinue();
                    } else {
                        StopAndTurnBack();
                    }
                }

            } else {
                // If not at the end of the line or at the stopping point, move at the set speed
                moveSpeed = Mathf.SmoothStep(moveSpeed, maxSpeed, acceleration * GameUtility.timeDelta);
            }

            // Move along spline
            travelledDistance += (moveSpeed * direction) * GameUtility.timeDelta;


            // If the object has reached the end of the spline, go to the next one
            while (goingForward && travelledDistance > spline.length && spline.nextSpline != null) {
                travelledDistance -= spline.length;
                spline = spline.nextSpline;
            } while (!goingForward && travelledDistance < 0f && spline.previousSpline != null) {
                travelledDistance += spline.length;
                spline = spline.previousSpline;
            }


            // Move
            MoveToPoint(travelledDistance);
        }

        private void Start() {
            MoveToPoint(0);
        }

        public enum MovementDirection {
            Backward = -1,
            None = 0,
            Forward = 1
        }

    }
}