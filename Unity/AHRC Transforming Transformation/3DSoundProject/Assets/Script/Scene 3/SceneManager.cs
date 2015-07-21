using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TBE_3DCore;

public class SceneManager : MonoBehaviour {

    public GameObject mainGraphicWindow;
    public GameObject controlPanel;

    public GameObject myHand;

    private float distance = 0.0f;
    public GameObject SpherePattern;

    public Transform soundIcon;
    private AudioClip[] myAudioClips;

    public GameObject MusicObjGroup;

    //private Vector2 ResolutionMax;

    private Camera cam;
    private Vector2 point;
    private RectTransform myHandRectTransform;

    private GameObject Sphere;
    private GameObject DraggedIcon = null;
    public GameObject DraggedObj = null;




    /************************************************************************************************************/
	// Use this for initialization
	void Start () {


        //ResolutionMax = controlPanel.transform.parent.GetComponent<CanvasScaler>().referenceResolution;
        //Debug.Log("Max Resolution: " + ResolutionMax + " ---  Screen: " + Screen.width + " " + Screen.height);

        //Use to refine  of cursor in panel 
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        myHandRectTransform = GameObject.Find("HandIcon").GetComponent<RectTransform>();

        SetIconsfromSoundResources();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        /************************************************************************************************/
        //Manage the depth of the KINECT hand in the environment
        distance += Input.GetAxis("Mouse ScrollWheel");
        //Clamp Distance
        distance = Mathf.Clamp(distance, GameObject.Find("Front").transform.position.z + SpherePattern.transform.lossyScale.z / 2, GameObject.Find("Back").transform.position.z - SpherePattern.transform.lossyScale.z / 2);
        /************************************************************************************************/

        /************************************************************************************************/
        
        //if we have selected an icon and we drag it into the environment
        if (DraggedIcon)
        {
            //On Drag Object
            //DraggedIcon.transform.position = new Vector2(Input.mousePosition.x * Screen.width/ResolutionMax.x,Input.mousePosition.y * Screen.height/ResolutionMax.y);
            DraggedIcon.transform.position = Input.mousePosition;

            //If Mouse cursor is within the mainGraphicPanel
            //if (mainGraphicWindow.GetComponent<RectTransform>().rect.Contains(DraggedIcon.transform.position))
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mainGraphicWindow.GetComponent<RectTransform>(), Input.mousePosition, null, out point);
            if (mainGraphicWindow.GetComponent<RectTransform>().rect.Contains(point))
            {
                //Set Sphere as Active
                Sphere.SetActive(true);

                //For Kinect2.0
                ControlSoundObj(Sphere);

                //Set the Icon on Dragged Object
                SetIconOnObject(DraggedIcon, Sphere);
            }

            // the mouse cursor is located outside the mainGraphicPanel
            else
            {
                //Set Sphere as Inactive
                Sphere.SetActive(false);
            }
        }

        //If we have selected an object and we dragged it through th eenvironment
        if (DraggedObj)
        {
            //For Kinect 2.0
            ControlSoundObj(DraggedObj);

        }

        if (!Input.GetMouseButton(0))
        {
            //Button is release - sound is released in the environment
            if (DraggedIcon)
                OnUpObj(DraggedIcon);

        }
        /************************************************************************************************/
    
    }

    /************************************************************************************************************/
    /************************************************************************************************************/
    // Generic Functions
    /************************************************************************************************************/
    void SetIconsfromSoundResources()
    {
        int row = 0;

        //Load all the clips from the Resources\Clips
        myAudioClips = Resources.LoadAll<AudioClip>("Clips");

        //For each clip
        for (int i = 0; i < myAudioClips.Length; i++)
        {
            //Instantiate the prefab
            Transform iconObj = Instantiate(soundIcon);

            //Set it up in the hierarchy
            iconObj.SetParent(controlPanel.transform);

            //Name the IconObj
            iconObj.name = myAudioClips[i].name;

            //Set the Icon text
            iconObj.GetChild(0).GetComponent<Text>().text = myAudioClips[i].name;

            //Activate IconObj
            iconObj.gameObject.SetActive(true);

            /*********************************************************/
            //Set RectTransform
            //we maximize the number of icon per row to 2
            if (i % 2 == 0)
                row++;

            float yOffset = iconObj.parent.GetComponent<RectTransform>().rect.height / 12;
            //Position                                             //We do this below to hack the crappy Rect functionnalities of Unity - if Anchor is Left top corner of the rect transform not of the parent
            iconObj.GetComponent<RectTransform>().localPosition = new Vector3(iconObj.parent.GetComponent<RectTransform>().rect.width / 4 * (1 + 2 * (i % 2)), -iconObj.parent.GetComponent<RectTransform>().rect.height / 7 * (row) + yOffset, 0.0f);

            //Scale
            iconObj.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            /*********************************************************/
        }
    }

    /************************************************************************************************************/
    // Generic Public Function
    /************************************************************************************************************/
    public //Set State of collider of Musical Objects
    void SetAllColliderState(bool state)
    {
        Debug.Log("collider state is " + state);
        for (int i = 0; i < MusicObjGroup.transform.childCount; i++)
            MusicObjGroup.transform.GetChild(i).GetComponent<Collider>().enabled = state;
    }

    //Control Object in 3D environment
    public void ControlSoundObj(GameObject obj)
    {
        //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
        Vector2 p = new Vector2((mainGraphicWindow.transform.parent.GetComponent<RectTransform>().rect.width / Screen.width) * (Input.mousePosition.x - mainGraphicWindow.GetComponent<RectTransform>().position.x) / mainGraphicWindow.GetComponent<RectTransform>().rect.width,
                                (mainGraphicWindow.transform.parent.GetComponent<RectTransform>().rect.height / Screen.height) * (Input.mousePosition.y - mainGraphicWindow.GetComponent<RectTransform>().position.y) / mainGraphicWindow.GetComponent<RectTransform>().rect.height);
        //Clamp the value of p
        p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
        p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);

        //Projection from Ortho Camera (Viewport) to Perspective Camera
        //Ray rayPerspective = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        //For Kinect Manipulation we project from an orthoCamera 
        Ray rayPerspective = GameObject.Find("OrthoCamera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.green);

        //Vector3 vec = GameObject.Find("SceneCamera").GetComponent<Camera>().ViewportToWorldPoint(new Vector3(p.x, p.y, GameObject.Find("Scene Camera").transform.GetComponent<Camera>().nearClipPlane + GameObject.Find("Floor").transform.lossyScale.z * 10 / 2 + distance));
        //For Kinect Manipulation we project from an orthoCamera 
        Vector3 vec = GameObject.Find("OrthoCamera").GetComponent<Camera>().ViewportToWorldPoint(new Vector3(p.x, p.y, GameObject.Find("Scene Camera").transform.GetComponent<Camera>().nearClipPlane + GameObject.Find("Floor").transform.lossyScale.z * 10 / 2 + distance));

        //Clamp vec position
        vec = new Vector3(Mathf.Clamp(vec.x, GameObject.Find("Left").transform.position.x + obj.transform.lossyScale.x / 2, GameObject.Find("Right").transform.position.x - obj.transform.lossyScale.x / 2),
                          Mathf.Clamp(vec.y, GameObject.Find("Floor").transform.position.y + obj.transform.lossyScale.y / 2, GameObject.Find("Ceiling").transform.position.y - obj.transform.lossyScale.y / 2),
                          Mathf.Clamp(vec.z, GameObject.Find("Front").transform.position.z + obj.transform.lossyScale.z / 2, GameObject.Find("Back").transform.position.z - obj.transform.lossyScale.z / 2));

        obj.transform.position = vec;
    }


    //Set the position and scale of dragged icon 
    public void SetIconOnObject(GameObject dragIcon, GameObject obj)
    {
        
        //Set Icon size as a function of the position in depth of the Sphere
        dragIcon.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f) * (Vector3.Distance(GameObject.Find("Scene Camera").transform.position, GameObject.Find("Back").transform.position)) / (2 * Vector3.Distance(GameObject.Find("Scene Camera").transform.position, obj.transform.position));

        //Project the Dragged Icon onto the screen 
        Vector3 posViewport = GameObject.Find("Scene Camera").GetComponent<Camera>().WorldToViewportPoint(obj.transform.position);

        Vector3 posScreen = GameObject.Find("Main Camera").GetComponent<Camera>().ViewportToScreenPoint(new Vector3(posViewport.x * mainGraphicWindow.GetComponent<RectTransform>().anchorMax.x, posViewport.y, 0.0f));

        dragIcon.transform.position =posScreen;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainGraphicWindow.GetComponent<RectTransform>(), Input.mousePosition, null, out point);
        if (!mainGraphicWindow.GetComponent<RectTransform>().rect.Contains(point))
        {
            dragIcon.transform.position = new Vector3(Input.mousePosition.x,
                                                      Input.mousePosition.y,
                                                      0.0f);
        }
    }
    /************************************************************************************************************/
    /**********************************************************************/
    // Select/Release for Dragging Sound into Scene from control Panel
    /**********************************************************************/

    public void OnDownObj(GameObject obj)
    {
        //Duplicate the Icon Object
        DraggedIcon = Instantiate<GameObject>(obj);

        //Rename Object
        DraggedIcon.name = obj.name + "_drag_icon";
        DraggedIcon.transform.SetParent(obj.transform.parent);

        //Set As Active
        DraggedIcon.SetActive(true);

        //Set degree of transparency
        DraggedIcon.GetComponent<RawImage>().color = new Color(DraggedIcon.GetComponent<RawImage>().color.r, DraggedIcon.GetComponent<RawImage>().color.g, DraggedIcon.GetComponent<RawImage>().color.b, 0.25f);
        /**********************************************************************/

        //Create Sphere
        Sphere = Instantiate<GameObject>(GameObject.Find("Sphere"));
        Sphere.name = "Sound_" + obj.name;
        //Set Random color to Sphere
        Sphere.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        //Edit 3D text mesh
        Sphere.transform.GetChild(0).GetComponent<TextMesh>().text = obj.name;

        //Inactivate All Musical Object Collider - So we can place two sound at the same place
        SetAllColliderState(false);
    }

    public void OnUpObj(GameObject obj)
    {
        //Delete the Icon Obj
        Destroy(GameObject.Find(obj.name));
        //Reset Dragged Icon
        DraggedIcon = null;

        /**********************************************************************/
        //Activate All Musical Object Collider
        SetAllColliderState(true);
        /**********************************************************************/

        //If the mouse cursor is outside the mainGraphicPanel when button is up, the sphere is destroyed
        /*if (!mainGraphicWindow.GetComponent<RectTransform>().rect.Contains(GameObject.Find(obj.name).transform.position))
            Destroy(Sphere);*/

        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainGraphicWindow.GetComponent<RectTransform>(), Input.mousePosition, null, out point);
        if (!mainGraphicWindow.GetComponent<RectTransform>().rect.Contains(point))
            Destroy(Sphere);

        else // The Sphere is naturally placed in the environment
        {
            //Set obj in Hierarchy
            Sphere.transform.parent = MusicObjGroup.transform;

            //Assign to Layer
            Sphere.layer = 8;

            //Activate Object collider
            Sphere.GetComponent<Collider>().enabled = true;

            //Create the 3Dception Sound component
            Sphere.AddComponent<TBE_Source>();

            //Debug.Log("Release Object: " + obj.name + " and " + Sphere.name);

            //Get the corresponding audio clip - traverse the list of clip
            for (int i = 0; i < myAudioClips.Length; i++)
            {
                //Assign Audio Clip to TBE source
                if (obj.name == myAudioClips[i].name + "_drag_icon")
                {
                    Sphere.GetComponent<TBE_Source>().clip = myAudioClips[i];
                    Sphere.GetComponent<TBE_Source>().Play();
                    break;
                }
            }
        }
    }

    /**********************************************************************/
    // Select/Release forMoving Sound in Scene
    /**********************************************************************/
    public void SelectSoundObj(GameObject myPanel)
    {
        //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
        Vector2 p = new Vector2((myPanel.transform.parent.GetComponent<RectTransform>().rect.width / Screen.width) * (myHandRectTransform.position.x/*Input.mousePosition.x*/ - myPanel.GetComponent<RectTransform>().position.x) / myPanel.GetComponent<RectTransform>().rect.width,
                                (myPanel.transform.parent.GetComponent<RectTransform>().rect.height / Screen.height) * (myHandRectTransform.position.y/*Input.mousePosition.y*/ - myPanel.GetComponent<RectTransform>().position.y) / myPanel.GetComponent<RectTransform>().rect.height);

        //Clamp the value of p
        p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
        p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);

        //Projection from main ortho Camera (Viewport) to Perspective Camera
        Ray rayPerspective = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.red);

        //Debug.Log(Vector3.Distance(rayPerspective.origin, myHand.transform.position) + 0.5f);

        //Raycast test on sound object
        RaycastHit hit;
        if (Physics.Raycast(rayPerspective, out hit, Vector3.Distance(rayPerspective.origin, myHand.transform.position) + 0.5f))
        {
            //Only if hit object is a musical object
            if (hit.collider.gameObject.layer == 8)
            {
                if (hit.distance > Vector3.Distance(rayPerspective.origin, myHand.transform.position) - 0.6f)
                {
                    DraggedObj = hit.collider.gameObject;
                    //Debug.Log("Selected Sound Object: " + DraggedObj.name);

                    //Deactivate Collider
                    DraggedObj.GetComponent<Collider>().enabled = false;

                    //Inactivate All Musical Object Collider - So we can place two sound at the same place
                    SetAllColliderState(false);
                }
            }
        }
    }

    public void ReleaseSoundObj(GameObject myPanel)
    {
        
        if (DraggedObj)
        {
            Debug.Log("Release: " + DraggedObj);
            //Reset collider
            DraggedObj.GetComponent<Collider>().enabled = true;

            //Reset Dragged Obj
            DraggedObj = null;
        }

        //Activate All Musical Object Collider 
        SetAllColliderState(true);
    }

    /**********************************************************************/
    // Drop Sounds to Recycle Bin
    /**********************************************************************/
    public void RecycleSound(GameObject bin)
    {
        if (DraggedObj)
        {
            Debug.Log("Recycle Object: " + bin.name);
            //Destroy Dragged Object
            Destroy(DraggedObj);

            //Reset DraggedObj
            DraggedObj = null;
        }

        //Activate All Musical Object Collider 
        SetAllColliderState(true);
    }
}
