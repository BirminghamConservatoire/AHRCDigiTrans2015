using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RectTest : MonoBehaviour {

    private RectTransform rectTransform;
    private Camera cam;
    private Vector2 point;

    //private RectTransform myHandRectTransform;
    private RectTransform myRightHandRectTransform;

    private SceneManager mySceneManagerScript;

    private myKinectManager myKinectManagerScript;

    // Use this for initializationC:\Users\M.Poyade\Documents\GitHub\AHRCDigiTrans2015\Unity\AHRC Transforming Transformation\3DSoundProject\Assets\Script\Scene 1 & 2\Manage3DSounds.cs
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        
        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();

        myKinectManagerScript = GameObject.Find("SceneObjects").GetComponent<myKinectManager>();

        myRightHandRectTransform = myKinectManagerScript.RightHandIcon.GetComponent<RectTransform>();
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, myRightHandRectTransform.position, null, out point);
        if (rectTransform.rect.Contains(point))
        {
            //GameObject.Find("DebugTextKinect").GetComponent<Text>().text = "KinectHand on: " + transform.name;

            //If an icon is clicked
            if (myKinectManagerScript.selectionStateMachine == 1 && rectTransform.gameObject.tag == "icon")
            {
                //Debug.Log("Icon selected with right Hand: " + rectTransform.gameObject.name);

                //Icon Selected
                mySceneManagerScript.OnDownObj(transform.gameObject);

                //Reset the state machine
                myKinectManagerScript.selectionStateMachine = 0.0f;
            }

            //If Hand state from open to close -> Select actions
            if (myKinectManagerScript.selectionStateMachine == 1 && rectTransform.gameObject.tag == "main window")
            {
                //Debug.Log("Right hand select in Main Window");

                //check for object selection
                mySceneManagerScript.SelectSoundObj(rectTransform.gameObject, myKinectManagerScript.RightHandObj, myKinectManagerScript.RightHandIcon);
                
                //Reset the state machine
                myKinectManagerScript.selectionStateMachine = 0.0f;
            }

            //If Hand state from close to open -> Release actions
            if (myKinectManagerScript.selectionStateMachine == -1)
            {
                //If we have selected an object and we dragged it in the eenvironment
                if (mySceneManagerScript.DraggedObj)
                {
                    //Debug.Log("Release Obj " + rectTransform.gameObject);

                    if (rectTransform.gameObject.tag != "bin")
                        mySceneManagerScript.ReleaseSoundObj(rectTransform.gameObject);
                    else//Release Obj into the bin
                        mySceneManagerScript.RecycleSound(rectTransform.gameObject);

                    //Reset the state machine
                    myKinectManagerScript.selectionStateMachine = 0.0f;
                }
            }
        }
    }
}
