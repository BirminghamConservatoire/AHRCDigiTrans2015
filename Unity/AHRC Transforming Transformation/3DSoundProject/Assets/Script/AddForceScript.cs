using UnityEngine;
using System.Collections;

public class AddForceScript : MonoBehaviour {
    public float amplitude;

   void OnMouseDown ()
   {
       //Add force as a function of the local z axis of the camera
       GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 1000 * amplitude, ForceMode.Force);
   }
}
