using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
//3DCeption Library
using TBE_3DCore;

public class SceneManager : MonoBehaviour {


    public GameObject mainGraphicWindow;
    public GameObject controlPanel;

    public GameObject SpherePattern;

    public Transform soundIcon;
    public AudioClip[] myAudioClips;

    public GameObject MusicObjGroup;

    //private Vector2 ResolutionMax;

    private Camera cam;
    private Vector2 point;

    private GameObject Sphere;
    private GameObject DraggedIcon = null;
    public GameObject DraggedObj = null;

    private myKinectManager myKinectManagerScript;

    /************************************************************************************************************/
	// Use this for initialization
	void Start () {

        //ResolutionMax = controlPanel.transform.parent.GetComponent<CanvasScaler>().referenceResolution;
        //Debug.Log("Max Resolution: " + ResolutionMax + " ---  Screen: " + Screen.width + " " + Screen.height);

        //Use to refine  of cursor in panel 
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        //Load Audio Samples
        LoadAudioSamples();

        myKinectManagerScript = GameObject.Find("SceneObjects").GetComponent<myKinectManager>();
	}


    void FixedUpdate()
    {

        /************************************************************************************************/
        //if we have selected an icon and we drag it into the environment
        if (DraggedIcon)
        {
            //Debug.Log("Icon Selection");
            ManageIconDragging(DraggedIcon, myKinectManagerScript.RightHandIcon.transform.position);
        }

        /***************************************************************************************************/

        //Works only  when the hand is releasing 
        if ( myKinectManagerScript.selectionStateMachine == -1)
        {
            //hand comes from close to open (myKinectManagerScript.selectionStateMachine = -1) - sound is released in the environment
            if (DraggedIcon)
			{
                OnUpObj(DraggedIcon, myKinectManagerScript.RightHandIcon.transform.position);

				SyncAudioSources(Sphere);
			}
        }

		/************************************************************************************************/

        //If we have selected an object and we dragged it through the environment
        if (DraggedObj)
        {
            ControlSoundObj(DraggedObj, myKinectManagerScript.RightHandIcon.transform.position);
        }

        /***************************************************************************************************/

    }

    /************************************************************************************************************/
    /************************************************************************************************************/
    // Generic Functions
    /************************************************************************************************************/

    void LoadAudioSamples()
    {
        /*********************************************************/
        //Get Files
        /*********************************************************/
        DirectoryInfo directoryInformation = new DirectoryInfo("AudioClips");
        if (!directoryInformation.Exists)
            directoryInformation.Create();

        FileInfo[] fileList = directoryInformation.GetFiles();
        //Debug.Log ("Number of Files: " + fileList.Length);
        /*********************************************************/

        
        myAudioClips = new AudioClip[fileList.Length];
        
        for (int i = 0; i < myAudioClips.Length; i++)
        {
            /*********************************************************/
            //Load & Store Audio Files 
            /*********************************************************/
            string url;
            
            #if UNITY_STANDALONE_WIN
                url= "file:///" + fileList[i].FullName;
            #endif

            #if UNITY_EDITOR_WIN
                url = "file:///" + fileList[i].FullName;
            #endif

            #if UNITY_STANDALONE_OSX
                url = "file://" + fileList[i].FullName;
            #endif

            #if UNITY_EDITOR_OSX
                url = "file://" + fileList[i].FullName;
            #endif


            url = url.Replace("\\", "/");
            WWW www = new WWW(url);

            AudioClip clip = www.GetAudioClip(false);

            while (clip.loadState != AudioDataLoadState.Loaded)
            {
                //Debug.Log("Loading... " + clip.name);
            }

            clip.name = Path.GetFileName(url);
            //Debug.Log("done loading... " + clip.name);

            myAudioClips[i] = clip;
            /*********************************************************/

            /*********************************************************/
            //Organize Audio Samples Icons in Interface
            /*********************************************************/
            SetIconsfromSoundResources(i);
            /*********************************************************/
        }   
    }


    private int row = 0;
    
    void SetIconsfromSoundResources(int i)
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
        /*********************************************************/
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

    void SyncAudioSources(GameObject sphere)
    {
        // If we already at least one sound object in the environment
        if (MusicObjGroup.transform.childCount > 1)
        {
            // Sync the sample number for the passed-in sphere to the first-added sphere
            sphere.GetComponent<TBE_Source>().timeSamples = MusicObjGroup.transform.GetChild(0).GetComponent<TBE_Source>().timeSamples;
        }
    }

    /************************************************************************************************************/
    // Generic Public Function
    /************************************************************************************************************/
    public //Set State of collider of Musical Objects
    void SetAllColliderState(bool state)
    {
        //Debug.Log("collider state is " + state);
        for (int i = 0; i < MusicObjGroup.transform.childCount; i++)
            MusicObjGroup.transform.GetChild(i).GetComponent<Collider>().enabled = state;
    }

    //Control Object in 3D environment
    public void ControlSoundObj(GameObject obj, Vector3 myInput)
    {
        //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
        Vector2 p = new Vector2((mainGraphicWindow.transform.parent.GetComponent<RectTransform>().rect.width / Screen.width) * (myInput.x - mainGraphicWindow.GetComponent<RectTransform>().position.x) / mainGraphicWindow.GetComponent<RectTransform>().rect.width,
                                (mainGraphicWindow.transform.parent.GetComponent<RectTransform>().rect.height / Screen.height) * (myInput.y - mainGraphicWindow.GetComponent<RectTransform>().position.y) / mainGraphicWindow.GetComponent<RectTransform>().rect.height);
        
        //Clamp the value of p
        p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
        p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);


        //For Kinect Manipulation we project from an orthoCamera 
        Ray rayPerspective = GameObject.Find("OrthoCamera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.green);

        Vector3 vec = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportToWorldPoint(new Vector3(p.x, p.y, GameObject.Find("Scene Camera").transform.GetComponent<Camera>().nearClipPlane + GameObject.Find("Floor").transform.lossyScale.z * 10 / 2 + myKinectManagerScript.distance));
        
        //Clamp vec position
        vec = new Vector3(Mathf.Clamp(vec.x, GameObject.Find("Left").transform.position.x + obj.transform.lossyScale.x / 2, GameObject.Find("Right").transform.position.x - obj.transform.lossyScale.x / 2),
                          Mathf.Clamp(vec.y, GameObject.Find("Floor").transform.position.y + obj.transform.lossyScale.y / 2, GameObject.Find("Ceiling").transform.position.y - obj.transform.lossyScale.y / 2),
                          Mathf.Clamp(vec.z, GameObject.Find("Front").transform.position.z + obj.transform.lossyScale.z / 2, GameObject.Find("Back").transform.position.z - obj.transform.lossyScale.z / 2));

        obj.transform.position = vec;
    }


    //Set the position and scale of dragged icon 
    public void SetIconOnObject(GameObject dragIcon, GameObject obj, Vector3 myInput)
    {
        //Set Icon size as a function of the position in depth of the Sphere
        dragIcon.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f) * (Vector3.Distance(GameObject.Find("Scene Camera").transform.position, GameObject.Find("Back").transform.position)) / (2 * Vector3.Distance(GameObject.Find("Scene Camera").transform.position, obj.transform.position));

        //Project the Dragged Icon onto the screen 
        Vector3 posViewport = GameObject.Find("Scene Camera").GetComponent<Camera>().WorldToViewportPoint(obj.transform.position);

        Vector3 posScreen = GameObject.Find("Main Camera").GetComponent<Camera>().ViewportToScreenPoint(new Vector3(posViewport.x * mainGraphicWindow.GetComponent<RectTransform>().anchorMax.x, posViewport.y, 0.0f));

        dragIcon.transform.position =posScreen;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainGraphicWindow.GetComponent<RectTransform>(), myInput, null, out point);
        if (!mainGraphicWindow.GetComponent<RectTransform>().rect.Contains(point))
            dragIcon.transform.position = new Vector3(myInput.x, myInput.y, 0.0f);
    }
    /************************************************************************************************************/
    /**********************************************************************/
    // Select/Release for Dragging Sound into Scene from control Panel
    /**********************************************************************/

    public void OnDownObj(GameObject obj)
    {
        //Debug.Log("Create Icon Object");
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

    public void OnUpObj(GameObject obj, Vector3 myInput)
    {
        //Delete the Icon Obj
        Destroy(GameObject.Find(obj.name));
        
        //Reset Dragged Icon
        DraggedIcon = null;

        /**********************************************************************/
        //Activate All Musical Object Collider
        /**********************************************************************/
        SetAllColliderState(true);
        /**********************************************************************/

        //If the cursor is outside the mainGraphicPanel when button is up, the sphere is destroyed
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainGraphicWindow.GetComponent<RectTransform>(), myInput, null, out point);
        if (!mainGraphicWindow.GetComponent<RectTransform>().rect.Contains(point))
        {
            //Debug.Log("Destoy out of Main Window");
            Destroy(Sphere);
        }
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
            Sphere.GetComponent<TBE_Source>().loop = true;

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
    // Manage Icon Drag
    /**********************************************************************/
    private void ManageIconDragging(GameObject IconDrag, Vector3 myInput)
    {
        //On Drag Object
        IconDrag.transform.position = myInput;

        //If cursor is within the mainGraphicPanel
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainGraphicWindow.GetComponent<RectTransform>(), myInput, null, out point);
        if (mainGraphicWindow.GetComponent<RectTransform>().rect.Contains(point))
        {
            //Set Sphere as Active
            Sphere.SetActive(true);

            //For Kinect2.0
            ControlSoundObj(Sphere, myInput);

            //Set the Icon on Dragged Object
            SetIconOnObject(IconDrag, Sphere, myInput);
        }

        // the cursor is located outside the mainGraphicPanel
        else
        {
            //Set Sphere as Inactive
            Sphere.SetActive(false);
        }
    }


    /**********************************************************************/
    // Select/Release forMoving Sound in Scene
    /**********************************************************************/
    public void SelectSoundObj(GameObject myPanel, GameObject handObj, GameObject handIcon)
    {
        //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
        Vector2 p = new Vector2((myPanel.transform.parent.GetComponent<RectTransform>().rect.width / Screen.width) * (handIcon.GetComponent<RectTransform>().position.x - myPanel.GetComponent<RectTransform>().position.x) / myPanel.GetComponent<RectTransform>().rect.width,
                                (myPanel.transform.parent.GetComponent<RectTransform>().rect.height / Screen.height) * (handIcon.GetComponent<RectTransform>().position.y - myPanel.GetComponent<RectTransform>().position.y) / myPanel.GetComponent<RectTransform>().rect.height);

        //Clamp the value of p
        p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
        p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);

        //Projection from main ortho Camera (Viewport) to Perspective Camera
        Ray rayPerspective = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.red);

        //Debug.Log(Vector3.Distance(rayPerspective.origin, handObj.transform.position) + 0.5f);

        //Raycast test on sound object
        RaycastHit hit;
        if (Physics.Raycast(rayPerspective, out hit, Vector3.Distance(rayPerspective.origin, handObj.transform.position) + 0.5f))
        {
            //Only if hit object is a musical object
            if (hit.collider.gameObject.layer == 8)
            {
                if (hit.distance > Vector3.Distance(rayPerspective.origin, handObj.transform.position) - 0.6f)
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
            //Debug.Log("Release: " + DraggedObj);
            
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
