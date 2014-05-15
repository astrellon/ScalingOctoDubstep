using UnityEngine;
using System;

namespace Space
{
    public class EngineObject : MonoBehaviour 
    {
        public float MaxThrust = 1.0f;
        public float Thrust = 0.0f;

        public EngineObject()
        {

        }

        public void Update()
        {
            Rigidbody body = GetComponent<Rigidbody>(); 
            if (body != null && Thrust != 0.0f)
            {
                Vector3 force = new Vector3(0, 0, Thrust);
                body.AddRelativeForce(force);
            }
            /*
            Rigidbody body = GetComponent<Rigidbody>(); 
            if (body != null && Thrust != 0.0f)
            {
                Vector3 position = transform.position;
                Vector3 force = transform.forward * Thrust;
                Debug.Log("Position: " + transform.position);
                if (EngineNode != null)
                {
                    position = EngineNode.transform.position;
                    Debug.Log("EngineNode! " + position);
                    Transform node = EngineNode.transform.parent;
                    while (node != null && node != transform)
                    {
                        Debug.Log("EngineNode Parent!");
                        position = node.InverseTransformPoint(position);
                        force = node.InverseTransformDirection(force);
                        node = node.parent;
                    }
                }
                position = new Vector3(0,0 ,0);
                Debug.Log("Applying force: " + force + " at " + position);
                body.AddForceAtPosition(force, position);

            }
            */
            
        }
    }
}
