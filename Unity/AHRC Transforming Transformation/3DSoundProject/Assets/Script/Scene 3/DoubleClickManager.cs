using UnityEngine;
using System.Collections;

public class DoubleClickManager : MonoBehaviour {

    /**********************************/
    //double Click
    /**********************************/
    private float lastClickTime;
    private float catchTime = 0.5f;

    public bool doubleClickOn = false;
    public bool doubleClickOff = false;

    private int clickCount = 0;
    private int doubleClickCount = 0;
    /**********************************/

    private SceneManager mySceneManagerScript;

	// Use this for initialization
	void Start () {

        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();
	}
	
	// Update is called once per frame
	void LateUpdate () {

        /**************************************************************************/
        //Double Click 
        /**************************************************************************/
        if (Input.GetMouseButtonDown(0))
        {
            if (clickCount == 0)
            {
                //normal click - the first click is always a normal click
                //Debug.Log("Normal click 1");

                lastClickTime = Time.time;
                clickCount++;
            }
            else 
            {
                //Check if double click or second normal click
                if (Time.time - lastClickTime < catchTime)
                {
                    //double click
                    //Debug.Log("Double click");

                    if (doubleClickCount == 0)
                    {
                        //1st double Click - Selection
                        //Debug.Log("Double click ON");

                        doubleClickOn = true;
                        doubleClickOff = false;

                        doubleClickCount++;
                    }
                    else
                    {
                        //2nd double Click - Deselection
                        //Debug.Log("Double click OFF");

                        doubleClickOn = false;
                        doubleClickOff = true;

                        doubleClickCount = 0;
                    }


                    lastClickTime = 0.0f;
                    clickCount = 0;
                }
                else
                {
                    //normal click
                    //Debug.Log("Normal click 2");

                    lastClickTime = Time.time;
                    clickCount++;
                }
            }  
        }
        /**************************************************************************/

	}
}
