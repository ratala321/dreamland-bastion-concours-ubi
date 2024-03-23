using System.Collections;
using UnityEngine;
using System;


namespace Utils
{
    public class ShootingUtility :  MonoBehaviour
    {
    
        [SerializeField] public float TimeToFly;
        [SerializeField] public GameObject ObjectToFire;

        [SerializeField] public GameObject Destination;
        [SerializeField] public GameObject Origin;
        [SerializeField] public float startingAngle;
        public void Start()
        {
            FireBetween(Origin.transform.position, Destination.transform.position, startingAngle);
        }

        public void FireBetween(Vector3 startPosition, Vector3 endPosition, float radAngle)
        {
            Vector3 middlePosition = GetThirdPoint(startPosition, endPosition,radAngle);
            StartCoroutine(MoveObject(startPosition, middlePosition, endPosition));

        }
  
        private IEnumerator MoveObject(Vector3 startPoint, Vector3 middlePoint, Vector3 endPoint)
        {
            float timer = 0; 
            while (timer < TimeToFly)
            {
                timer += Time.deltaTime ;
                float ratio = Math.Min(timer / TimeToFly, 1.0f);
                ObjectToFire.transform.position = RunBezier(startPoint, middlePoint, endPoint, ratio);                
                yield return null;
            }
        }

        private Vector3 RunBezier(Vector3 startPoint, Vector3 middlePoint,Vector3 endPoint, float ratio )
        {
            Vector3 bezierPosition = (1-ratio) * startPoint + (2 * ratio)* (1-ratio) * middlePoint + ratio * ratio *endPoint;
            return bezierPosition;
        }
        private static Vector3 GetThirdPoint(Vector3 initpos, Vector3 targetPos, float startingAngle)
        {
            // On cherche le point du milieu entre init et target
            Vector3 midPoint = (targetPos + initpos) / 2;
        
            float distance = Vector3.Distance(initpos, midPoint);
            float oppositeLength = (float) Math.Tan(startingAngle) * distance; 
            Vector3 oppositeVector = Vector3.up * oppositeLength;
            Vector3 thirdPointPosition = midPoint + oppositeVector;
             
            return thirdPointPosition;
        }
        
    }
}