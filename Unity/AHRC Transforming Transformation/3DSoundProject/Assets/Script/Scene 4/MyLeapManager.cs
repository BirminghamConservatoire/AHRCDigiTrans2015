using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
//3DCeption Library
using TBE_3DCore;
using UnityEditor;
using Leap;

public class MyLeapManager : MonoBehaviour {

    public GameObject HandControllerObj;
    public GameObject RightHandIcon;

    public Texture HandFinger;
    public Texture HandFist;

    public float distance = 0.0f;

    private SceneManager mySceneManagerScript;

	// Use this for initialization
	void Start () {

        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();
	
	}
	
	// Update is called once per frame
	void Update () {


        //Debug.Log(HandControllerObj.GetComponent<HandController>().rightGraphicsModel.name); 

        //Debug.Log(GameObject.Find(HandControllerObj.GetComponent<HandController>().GetAllGraphicsHands()[0].name));
        if (GameObject.Find(HandControllerObj.GetComponent<HandController>().rightGraphicsModel.name + "(Clone)"))
        {
            //Debug.Log(GameObject.Find(HandControllerObj.GetComponent<HandController>().rightGraphicsModel.name + "(Clone)").transform.FindChild("HandContainer").position);

            //Project Kinect hand to the screen as Icon
            ProjectControlObjectOntoScreen(RightHandIcon, GameObject.Find(HandControllerObj.GetComponent<HandController>().rightGraphicsModel.name + "(Clone)"), GameObject.Find(HandControllerObj.GetComponent<HandController>().rightGraphicsModel.name + "(Clone)").transform.FindChild("HandContainer").position);
        }
	}

    public void ProjectControlObjectOntoScreen(GameObject projectedIcon, GameObject obj, Vector3 HandPos)
    {

        //Offset on X axis - we offset the position of the hand compared to the Kinect 
        //float offset = UnityEngine.Screen.width / 3.0f;

        //Project the Hand Icon from the Rw tracked to Viewport
        Vector3 posViewport = GameObject.Find("Scene Camera").GetComponent<Camera>().WorldToViewportPoint(HandPos);
        //Project the Hand Icon from the  Viewport to Screen
        Vector3 posScreen = GameObject.Find("Main Camera").GetComponent<Camera>().ViewportToScreenPoint(new Vector3(posViewport.x * 0.8f, posViewport.y, 0.0f));

        Debug.Log(posViewport + "      " + posScreen);

        //Offset on X
        //posScreen.x -= offset;

        //Assign To the Icon
        projectedIcon.transform.position = posScreen;
        

        //Projection of the icon position from Screen into the Real world as hand object
        //ControlObjWithLeap(projectedIcon, obj);
    }

    public void ControlObjWithLeap(GameObject Icon, GameObject obj)
    {
        //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
        Vector2 p = new Vector2((mySceneManagerScript.mainGraphicWindow.transform.parent.GetComponent<RectTransform>().rect.width / UnityEngine.Screen.width) * (Icon.transform.position.x - mySceneManagerScript.mainGraphicWindow.GetComponent<RectTransform>().position.x) / mySceneManagerScript.mainGraphicWindow.GetComponent<RectTransform>().rect.width,
                                (mySceneManagerScript.mainGraphicWindow.transform.parent.GetComponent<RectTransform>().rect.height / UnityEngine.Screen.height) * (Icon.transform.position.y - mySceneManagerScript.mainGraphicWindow.GetComponent<RectTransform>().position.y) / mySceneManagerScript.mainGraphicWindow.GetComponent<RectTransform>().rect.height);

        //Clamp the value of p
        p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
        p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);

        //Projection from Ortho Camera (Viewport) to Perspective Camera
        Ray rayPerspective = GameObject.Find("OrthoCamera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.blue);

        //For Kinect Manipulation
        Vector3 vec = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportToWorldPoint(new Vector3(p.x, p.y, GameObject.Find("Scene Camera").transform.GetComponent<Camera>().nearClipPlane + GameObject.Find("Floor").transform.lossyScale.z * 10 / 2 + distance));

        //Clamp vec position
        /*vec = new Vector3(Mathf.Clamp(vec.x, GameObject.Find("Left").transform.position.x + obj.transform.lossyScale.x / 2, GameObject.Find("Right").transform.position.x - obj.transform.lossyScale.x / 2),
                          Mathf.Clamp(vec.y, GameObject.Find("Floor").transform.position.y + obj.transform.lossyScale.y / 2, GameObject.Find("Ceiling").transform.position.y - obj.transform.lossyScale.y / 2),
                          Mathf.Clamp(vec.z, GameObject.Find("Front").transform.position.z + obj.transform.lossyScale.z / 2, GameObject.Find("Back").transform.position.z - obj.transform.lossyScale.z / 2));
        */
        obj.transform.position = vec;

        //Set Icon size as a function of the position in depth of the Sphere
        Icon.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f) * (Vector3.Distance(GameObject.Find("Scene Camera").transform.position, GameObject.Find("Back").transform.position)) / (2 * Vector3.Distance(GameObject.Find("Scene Camera").transform.position, obj.transform.position));


    }
    
}
