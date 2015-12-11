using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
//3DCeption Library
using TBE_3DCore;
//using UnityEditor;

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

    public GameObject volumeInterface; 

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

    void LateUpdate()
    {
        float hue;
        float sat;
        float val;

        //Check distance between object
        for (int i = 0; i < GameObject.Find("SoundsObjGrpInScene").transform.childCount; i++)
        {
            for (int j = 0; j < GameObject.Find("SoundsObjGrpInScene").transform.childCount; j++)
            {
                //Compare location distance
                float howFar = Vector3.Distance(GameObject.Find("SoundsObjGrpInScene").transform.GetChild(i).position, GameObject.Find("SoundsObjGrpInScene").transform.GetChild(j).position);

                if (howFar > 0.0f && howFar < SpherePattern.transform.lossyScale.z)
                {
                    //Highlight the object in contact    
                    float highlight = 1.0f;
                    /*EditorGUIUtility.*/RGBToHSV(GameObject.Find("SoundsObjGrpInScene").transform.GetChild(i).GetComponent<Renderer>().material.color, out hue, out sat, out val);
                    GameObject.Find("SoundsObjGrpInScene").transform.GetChild(i).GetComponent<Renderer>().material.color = /*EditorGUIUtility.*/HSVToRGB(hue, sat, highlight);
                    break;
                }
                else
                {
                    //Highlight the object in contact    
                    float highlight = 0.5f;
                    /*EditorGUIUtility.*/RGBToHSV(GameObject.Find("SoundsObjGrpInScene").transform.GetChild(i).GetComponent<Renderer>().material.color, out hue, out sat, out val);
                    GameObject.Find("SoundsObjGrpInScene").transform.GetChild(i).GetComponent<Renderer>().material.color = /*EditorGUIUtility.*/HSVToRGB(hue, sat, highlight);
                }
            }
        }
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
        iconObj.GetComponent<RectTransform>().localPosition = new Vector3(iconObj.parent.GetComponent<RectTransform>().rect.width / 4 * (1 + 2 * (i % 2)), -iconObj.parent.GetComponent<RectTransform>().rect.height / 5 * (row) + yOffset, 0.0f);

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
     //Set State of collider of Musical Objects
    /*publicvoid SetAllColliderState(bool state)
    {
       //Debug.Log("collider state is " + state);
       for (int i = 0; i < MusicObjGroup.transform.childCount; i++)
          MusicObjGroup.transform.GetChild(i).GetComponent<Collider>().enabled = state;
    }*/

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
        //Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.green);

        Vector3 vec = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportToWorldPoint(new Vector3(p.x, p.y, GameObject.Find("Scene Camera").transform.GetComponent<Camera>().nearClipPlane + GameObject.Find("Floor").transform.lossyScale.z * 10 / 2 + myKinectManagerScript.distance + SpherePattern.transform.lossyScale.z/2+ 0.05f)); // 0.05f: We add a tiny offset so that hand is in front of the text (nicer)

        //Clamp vec position
        vec = new Vector3(Mathf.Clamp(vec.x, GameObject.Find("Left").transform.position.x + obj.transform.lossyScale.x / 2, GameObject.Find("Right").transform.position.x - obj.transform.lossyScale.x / 2),
                          Mathf.Clamp(vec.y, GameObject.Find("Floor").transform.position.y + obj.transform.lossyScale.y / 2, GameObject.Find("Ceiling").transform.position.y - obj.transform.lossyScale.y / 2),
                          Mathf.Clamp(vec.z, GameObject.Find("Front").transform.position.z + obj.transform.lossyScale.z / 2, GameObject.Find("Back").transform.position.z - obj.transform.lossyScale.z / 2));

        obj.transform.position = vec;

    }

    //Control Volume of the Dragged Object - as a function of left hand position
    public void ControlObjVolume(GameObject obj, float deltaVol)
    {

        obj.GetComponent<TBE_Source>().volume += deltaVol;
    }


    private float h = 0.0f;
    private float s = 0.0f;
    private float v = 0.0f;

    //public Manage the Interface which show volume variation
    public void ManageVolumeInterface(bool show, GameObject obj)
    {
        volumeInterface.SetActive(show);

        if (obj != null && volumeInterface.activeSelf == true)
        {
            volumeInterface.transform.GetChild(0).GetComponent<Slider>().value = DraggedObj.GetComponent<TBE_Source>().volume;

            //Change the Color Saturation of Dragged Object
            /*EditorGUIUtility.*/RGBToHSV(obj.GetComponent<Renderer>().material.color,out h,out s, out v);
            //Debug.Log(h + "    " + s + "   " + "    " + v);
            s = 0.99f*volumeInterface.transform.GetChild(0).GetComponent<Slider>().value + 0.01f;//Sort of clamping the color - other when reconverting the hue is set to null
            DraggedObj.GetComponent<Renderer>().material.color = /*EditorGUIUtility.*/HSVToRGB(h, s, v);
        }
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
    // Manage Hand Icon Visibility
    /**********************************************************************/
    public void ManageHandIconVisibility(GameObject myPanel, GameObject handObj, GameObject handIcon)
    {
        //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
        Vector2 p = new Vector2((myPanel.transform.parent.GetComponent<RectTransform>().rect.width / Screen.width) * (handIcon.GetComponent<RectTransform>().position.x - myPanel.GetComponent<RectTransform>().position.x) / myPanel.GetComponent<RectTransform>().rect.width,
                                (myPanel.transform.parent.GetComponent<RectTransform>().rect.height / Screen.height) * (handIcon.GetComponent<RectTransform>().position.y - myPanel.GetComponent<RectTransform>().position.y) / myPanel.GetComponent<RectTransform>().rect.height);

        //Clamp the value of p
        p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
        p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);

        //Projection from main Camera (Viewport) to Perspective Camera
        Ray rayPerspective = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));//Returns a ray going from camera through a viewport point.
        Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 100, Color.blue);

        //Raycast test on sound object
        //If icon is in front of object - icon should be visible/If icon is in front of object - icon should be hidden
        RaycastHit hit;
        if (Physics.SphereCast(rayPerspective, 0.25f, out hit, Vector3.Distance(rayPerspective.origin, handObj.transform.position) - handObj.transform.localScale.z))
        {
            //Only if hit object is a musical object
            if (hit.collider.gameObject.layer == 8)
            {
                handIcon.GetComponent<RawImage>().enabled = false;
                Debug.Log(hit.collider.name);
            }
            else
                handIcon.GetComponent<RawImage>().enabled = true;
        }
        else
            handIcon.GetComponent<RawImage>().enabled = true;
    }


    /************************************************************************************************************/
    /**********************************************************************/
    // Select/Release for Dragging Sound into Scene from control Panel
    /**********************************************************************/
    private GameObject myLastSelectedIcon;
    public void OnDownObj(GameObject obj)
    {
        //Debug.Log("Create Icon Object");
        //Duplicate the Icon Object
        DraggedIcon = Instantiate<GameObject>(obj);

        myLastSelectedIcon = obj;

        //Rename Object
        DraggedIcon.name = obj.name + "_drag_icon";
        DraggedIcon.transform.SetParent(obj.transform.parent);

        //Set As Active
        DraggedIcon.SetActive(true);

        //Set degree of transparency
        DraggedIcon.GetComponent<RawImage>().color = new Color(DraggedIcon.GetComponent<RawImage>().color.r, DraggedIcon.GetComponent<RawImage>().color.g, DraggedIcon.GetComponent<RawImage>().color.b, 0.25f);
        
        //Grey the selected icon
        float hue;
        float sat;
        float val;

        /*EditorGUIUtility.*/RGBToHSV(obj.GetComponent<RawImage>().color, out hue, out sat, out val);
        obj.GetComponent<RawImage>().color = /*EditorGUIUtility.*/HSVToRGB(hue, sat, 0.5f);
  
        /**********************************************************************/

        //Create Sphere
        Sphere = Instantiate<GameObject>(SpherePattern);
        Sphere.name = "Sound_" + obj.name;
        
        //Set Random color to Sphere - Color is define in HSV 
        Sphere.GetComponent<Renderer>().material.color = /*EditorGUIUtility.*/HSVToRGB(Random.Range(0.0f, 1.0f), 1.0f, 0.5f);
        //Sphere.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

        //Edit 3D text mesh
        Sphere.transform.GetChild(0).GetComponent<TextMesh>().text = obj.name;

        //Inactivate All Musical Object Collider - So we can place two sound at the same place
        //SetAllColliderState(false);
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
        //SetAllColliderState(true);
        /**********************************************************************/

        //If the cursor is outside the mainGraphicPanel when button is up, the sphere is destroyed
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainGraphicWindow.GetComponent<RectTransform>(), myInput, null, out point);
        if (!mainGraphicWindow.GetComponent<RectTransform>().rect.Contains(point))
        {
            //Debug.Log("Destoy out of Main Window");
            Destroy(Sphere);

            //White back the selected icon
            float hue;
            float sat;
            float val;

            /*EditorGUIUtility.*/RGBToHSV(myLastSelectedIcon.GetComponent<RawImage>().color, out hue, out sat, out val);
            myLastSelectedIcon.GetComponent<RawImage>().color = /*EditorGUIUtility.*/HSVToRGB(hue, sat, 1.0f);
            
            //Reset Last selected Icon
            myLastSelectedIcon = null;
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

            //Disable the selection of the Icon
            myLastSelectedIcon.GetComponent<RectTest>().enabled = false;

            //Reset Last selected Icon
            myLastSelectedIcon = null;
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
        //Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 1000, Color.red);

        //Debug.Log(Vector3.Distance(rayPerspective.origin, handObj.transform.position) + 0.5f);

        //Raycast test on sound object
        RaycastHit hit;

        //TO BE CORRECTED - ONLY FOR DEBUG PURPOSE
        if (Physics.Raycast(rayPerspective, out hit, Vector3.Distance(rayPerspective.origin, handObj.transform.position) + handObj.transform.localScale.z))
        //if (Physics.Raycast(rayPerspective, out hit, GameObject.Find("Floor").transform.lossyScale.z * 10))// we want to able to grasp the object anytime the handicon is located in front of it - for a more antural interaction
        {
            //Only if hit object is a musical object
            if (hit.collider.gameObject.layer == 8)
            {

                //if (hit.distance > Vector3.Distance(rayPerspective.origin, handObj.transform.position) - 0.6f)
                {
                    DraggedObj = hit.collider.gameObject;
                    //Debug.Log("Selected Sound Object: " + DraggedObj.name);

                    //Deactivate Collider
                    DraggedObj.GetComponent<Collider>().enabled = false;

                    //Inactivate All Musical Object Collider - So we can place two sound at the same place
                    //SetAllColliderState(false);
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
        //SetAllColliderState(true);
    }

    /**********************************************************************/
    // Drop Sounds to Recycle Bin
    /**********************************************************************/
    public void RecycleSound(GameObject bin)
    {

        if (DraggedObj)
        {
            Debug.Log("Recycle Object: " + DraggedObj.transform.GetChild(0).GetComponent<TextMesh>().text + " in  " + bin.name);

            //Enable anew the corresponding Icon selection
            GameObject.Find(DraggedObj.transform.GetChild(0).GetComponent<TextMesh>().text).GetComponent<RectTest>().enabled = true;

            //White back the icon
            float hue;
            float sat;
            float val;

            /*EditorGUIUtility.*/RGBToHSV(GameObject.Find(DraggedObj.transform.GetChild(0).GetComponent<TextMesh>().text).GetComponent<RawImage>().color, out hue, out sat, out val);
            GameObject.Find(DraggedObj.transform.GetChild(0).GetComponent<TextMesh>().text).GetComponent<RawImage>().color = /*EditorGUIUtility.*/HSVToRGB(hue, sat, 1.0f);

            //Destroy Dragged Object
            Destroy(DraggedObj);

            //Reset DraggedObj
            DraggedObj = null;
        }

        //Activate All Musical Object Collider 
        //SetAllColliderState(true);
    }

    /**********************************************************************/
    // Drop Sounds to Recycle All Bin
    /**********************************************************************/
    public void RecycleAllSounds()
    {

        Debug.Log("Recycle All Objects");

        //For each icon - Enable selection and white back
        foreach (Transform icon in GameObject.Find("Panel").transform)
        {
            //Enable anew the corresonding Icon selection
            icon.GetComponent<RectTest>().enabled = true;

            //White back the icon
            float hue;
            float sat;
            float val;

            /*EditorGUIUtility.*/RGBToHSV(icon.GetComponent<RawImage>().color, out hue, out sat, out val);
            icon.GetComponent<RawImage>().color = /*EditorGUIUtility.*/HSVToRGB(hue, sat, 1.0f);
        }

        //for each object in Scene Destroy
        foreach (Transform child in GameObject.Find("SoundsObjGrpInScene").transform)
            Destroy(child.gameObject);

        if (DraggedObj)
        {
           
            //Destroy Dragged Object
            Destroy(DraggedObj);

            //Reset DraggedObj
            DraggedObj = null;
        }

        //Activate All Musical Object Collider 
        //SetAllColliderState(true);
    }


    /********************************************************************/
    //Convert color HSV to RGB to HSV
    /********************************************************************/
     public static Color HSVToRGB(float H, float S, float V)
     {
         if (S == 0f)
         {
             return new Color(V,V,V);
         }
         else if (V == 0.0f)
         {
             Color col = Color.black;
           return col;
         }
         else
         {
             Color col = Color.black;
             float Hval = H * 6f;
             int sel = Mathf.FloorToInt(Hval);
             float mod = Hval - sel;
             float v1 = V * (1f - S);
             float v2 = V * (1f - S * mod);
             float v3 = V * (1f - S * (1f - mod));
             switch (sel + 1)
             {
             case 0:
                 col.r = V;
                 col.g = v1;
                 col.b = v2;
                 break;
             case 1:
                 col.r = V;
                 col.g = v3;
                 col.b = v1;
                 break;
             case 2:
                 col.r = v2;
                 col.g = V;
                 col.b = v1;
                 break;
             case 3:
                 col.r = v1;
                 col.g = V;
                 col.b = v3;
                 break;
             case 4:
                 col.r = v1;
                 col.g = v2;
                 col.b = V;
                 break;
             case 5:
                 col.r = v3;
                 col.g = v1;
                 col.b = V;
                 break;
             case 6:
                 col.r = V;
                 col.g = v1;
                 col.b = v2;
                 break;
             case 7:
                 col.r = V;
                 col.g = v3;
                 col.b = v1;
                 break;
             }
             col.r = Mathf.Clamp(col.r, 0f, 1f);
             col.g = Mathf.Clamp(col.g, 0f, 1f);
             col.b = Mathf.Clamp(col.b, 0f, 1f);
             return col;
         }
     }

     public static void RGBToHSV(Color rgbColor, out float H, out float S, out float V)
     {
         if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
         {
             RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
         }
         else
         {
             if (rgbColor.g > rgbColor.r)
             {
                 RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
             }
             else
             {
                 RGBToHSVHelper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
             }
         }
     }

     private static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
     {
         V = dominantcolor;
         if (V != 0f)
         {
             float num = 0f;
             if (colorone > colortwo)
             {
                 num = colortwo;
             }
             else
             {
                 num = colorone;
             }
             float num2 = V - num;
             if (num2 != 0f)
             {
                 S = num2 / V;
                 H = offset + (colorone - colortwo) / num2;
             }
             else
             {
                 S = 0f;
                 H = offset + (colorone - colortwo);
             }
             H /= 6f;
             if (H < 0f)
             {
                 H += 1f;
             }
         }
         else
         {
             S = 0f;
             H = 0f;
         }
     }


}
