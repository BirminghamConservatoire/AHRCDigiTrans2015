using UnityEngine;
using System.Collections;

public class LeapRectTest : MonoBehaviour {
   
    private RectTransform rectTransform;
    private Camera cam;
    private Vector2 point;

    //private RectTransform myHandRectTransform;
    private RectTransform myRightHandRectTransform;

    private SceneManager mySceneManagerScript;

    private MyLeapManager myLeapManagerScript;

	// Use this for initialization
	void Start () {
        rectTransform = GetComponent<RectTransform>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();

        myLeapManagerScript = GameObject.Find("SceneObjects").GetComponent<MyLeapManager>();

        myRightHandRectTransform = myLeapManagerScript.RightHandIcon.GetComponent<RectTransform>();
	}
	
// Update is called once per frame
    void FixedUpdate()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, myRightHandRectTransform.position, null, out point);
        if (rectTransform.rect.Contains(point))
        {
            //Debug.Log(transform.name);
        }
    }
}
