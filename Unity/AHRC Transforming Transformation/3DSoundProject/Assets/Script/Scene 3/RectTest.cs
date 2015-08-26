using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RectTest : MonoBehaviour {

    private RectTransform rectTransform;
    private Camera cam;
    private Vector2 point;

    private RectTransform myHandRectTransform;

    private SceneManager mySceneManagerScript;

    private DoubleClickManager myDoubleClickManagerScript;

    // Use this for initializationC:\Users\M.Poyade\Documents\GitHub\AHRCDigiTrans2015\Unity\AHRC Transforming Transformation\3DSoundProject\Assets\Script\Scene 1 & 2\Manage3DSounds.cs
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        myHandRectTransform = GameObject.Find("HandIcon").GetComponent<RectTransform>();

        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();

        myDoubleClickManagerScript = GameObject.Find("Main Camera").GetComponent<DoubleClickManager>();
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, myHandRectTransform.position, null, out point);
        if (rectTransform.rect.Contains(point))
        {
            GameObject.Find("DebugText").GetComponent<Text>().text = "Cursor over: " + transform.name;

            //If an icon is clicked
            if (Input.GetMouseButtonDown(0) /*myDoubleClickManagerScript .doubleClickOn*/ && rectTransform.gameObject.tag == "icon")
            {
                //Debug.Log("Icon clicked: " + rectTransform.gameObject.name);
                //Icon Selected
                mySceneManagerScript.OnDownObj(transform.gameObject);

                myDoubleClickManagerScript.doubleClickOn = false;
            }

            //if click in the main window
            if (/*Input.GetMouseButtonDown(0)*/ myDoubleClickManagerScript.doubleClickOn && rectTransform.gameObject.tag == "main window")
            {
                //Debug.Log("click in Main Window");
                //check for object selection
                mySceneManagerScript.SelectSoundObj(rectTransform.gameObject);

                myDoubleClickManagerScript.doubleClickOn = false;
            }

            //If Mouse Button 1 is Up -> Release actions
            if (/*Input.GetMouseButtonUp(0)*/ myDoubleClickManagerScript.doubleClickOff)
            {
                //If we have selected an object and we dragged it in the eenvironment
                if (mySceneManagerScript.DraggedObj)
                {
                    //Debug.Log("Release Obj " + rectTransform.gameObject);
                    if (rectTransform.gameObject.tag != "bin")
                        mySceneManagerScript.ReleaseSoundObj(rectTransform.gameObject);
                    else//Release Obj into the bin
                        mySceneManagerScript.RecycleSound(rectTransform.gameObject);

                    myDoubleClickManagerScript.doubleClickOff = false;
                }
            }
        }
    }
}
