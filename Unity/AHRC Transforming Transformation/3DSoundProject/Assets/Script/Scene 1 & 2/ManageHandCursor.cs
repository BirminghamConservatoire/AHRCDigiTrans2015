using UnityEngine;
using System.Collections;

public class ManageHandCursor : MonoBehaviour {

    private Manage3DSounds mySoundManagerScript;

    public GameObject HandObj;
    private GameObject HandObjIcon;


    // Use this for initialization
    void Start()
    {
        mySoundManagerScript = GameObject.Find("Panel").GetComponent<Manage3DSounds>();

        HandObjIcon = transform.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

        //ControlHandCursor();
    }

    void ControlHandCursor()
    {

        mySoundManagerScript.ControlSoundObj(HandObj);

        //mySoundManagerScript.SetIconOnObject(HandObjIcon, HandObj);

        //Set hand cursor size as a function of the current z position of the hand in the world
        /*HandObjIcon.transform.localScale = new Vector3(2.0f, 2.0f, 1.0f) * (Vector3.Distance(GameObject.Find("Scene Camera").transform.position, GameObject.Find("Back").transform.position)) / (2 * Vector3.Distance(GameObject.Find("Scene Camera").transform.position, HandObj.transform.position));

        //Project the hand Icon on the screen 
        Vector3 posHandOnViewport = GameObject.Find("Scene Camera").GetComponent<Camera>().WorldToViewportPoint(GameObject.Find("Hand").transform.position);
        Vector3 posHandOnScreen = GameObject.Find("Main Camera").GetComponent<Camera>().ViewportToScreenPoint(new Vector3(posHandOnViewport.x * mySoundManagerScript.mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.x / Screen.width, posHandOnViewport.y * mySoundManagerScript.mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.y / Screen.height, 0.0f));
        HandObjIcon.transform.position = posHandOnScreen;*/

        //Set the position of the hand cursor as a function of mousePos(x,y) and distance
        /*if (!mySoundManagerScript.mainGraphicPanel.GetComponent<RectTransform>().rect.Contains(Input.mousePosition))
        {
            HandObjIcon.transform.position = new Vector3(Input.mousePosition.x,
                                                         Input.mousePosition.y,
                                                         0.0f);
        }*/

    }

}
