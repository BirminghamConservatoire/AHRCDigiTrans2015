using UnityEngine;
using System.Collections;

//We set the projection matrix of the camera scene so that the edges of the room are fitting into the screen

public class SetSceneCameraView : MonoBehaviour {

    Camera mySceneCamera;

    private float left;
    private float right;
    private float top;
    private float bottom;


	// Use this for initialization
	void Start () {
        
        //Get the Scene Camera
        mySceneCamera = transform.GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void LateUpdate () {

        //Set the Nearclip of the Scene Camera to the edge of the floor object
        mySceneCamera.nearClipPlane = Mathf.Abs (transform.localPosition.z + (GameObject.Find("Floor").transform.localScale.z * 10 / 2));

        //Set the Field of View angle of the Scen Camera
        //mySceneCamera.fieldOfView = Mathf.Atan((GameObject.Find("Floor").transform.localScale.x * 10 / 2) / mySceneCamera.nearClipPlane) * Mathf.Rad2Deg;

        //Set Matrix Parameters
        left = -1 * (GameObject.Find("Floor").transform.localScale.x * 10 / 2);
        right = GameObject.Find("Floor").transform.localScale.x * 10 / 2;
        bottom = -1 * (GameObject.Find("Front").transform.localScale.z * 10 / 2);
        top = GameObject.Find("Front").transform.localScale.z * 10 / 2;

        if (!GetComponent<Camera>().orthographic)
        {
            //Set the frustum on th Near Clip plane of the scene camera to the edge of the Interaction Volume - We just readjust the matrix
            Matrix4x4 m = RefineProjectionMatrix(left, right, bottom, top, mySceneCamera.nearClipPlane, mySceneCamera.farClipPlane);
            mySceneCamera.projectionMatrix = m;
        }
        else 
        {
            //Creates an orthogonal projection matrix.
            //static Matrix4x4 Ortho(float left, float right, float bottom, float top, float zNear, float zFar); 
            Matrix4x4 m = Matrix4x4.Ortho(left, right, bottom, top, mySceneCamera.nearClipPlane, mySceneCamera.farClipPlane);

            //Assign the orthogonal matrix to Camera
            GetComponent<Camera>().projectionMatrix = m;
        }
	}


    static Matrix4x4 RefineProjectionMatrix(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
}
