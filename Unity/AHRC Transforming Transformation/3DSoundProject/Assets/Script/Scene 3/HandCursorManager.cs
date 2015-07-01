using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HandCursorManager : MonoBehaviour {

    private SceneManager mySceneManagerScript;

    public GameObject HandObj;
    public GameObject HandObjIcon;

    public GameObject ObjectManager;

    public Texture HandFinger;
    public Texture HandFist;

	// Use this for initialization
	void Start () {

        mySceneManagerScript = ObjectManager.GetComponent<SceneManager>();
        Cursor.visible = false;
	}
	

	// Update is called once per frame
	void Update () {

        ControlHandCursor();
	}

    void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
            HandObjIcon.GetComponent<RawImage>().texture = HandFist;
        else
            HandObjIcon.GetComponent<RawImage>().texture = HandFinger;
    }

    void ControlHandCursor()
    { 
        //Control the hand cursor
        mySceneManagerScript.ControlSoundObj(HandObj);

        //Set the icon onto the control object
        mySceneManagerScript.SetIconOnObject(HandObjIcon, HandObj);
    }
}
