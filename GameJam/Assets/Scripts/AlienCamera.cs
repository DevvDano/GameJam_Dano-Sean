using UnityEngine;
using System.Collections;
    
    public class DampCamera2D : MonoBehaviour
    {
        public float smoothTime = 0.3F;
        private Vector3 velocity = Vector3.zero;
        [SerializeField] private Transform toCameraPos;
    
        //On PlayerCollision2D with Object tagged "CameraTrigger", Move Camera to Position of Object tagged "CameraPos"
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("CameraTrigger"))
            {
                if (toCameraPos != null)
        {
            // Define a target position above and behind the target transform
            Vector3 targetPosition = toCameraPos.TransformPoint(new Vector3(0, 5, -10));
    
            // Smoothly move the camera towards that target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
            }
        }
    }