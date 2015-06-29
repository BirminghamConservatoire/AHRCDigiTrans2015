using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TBE_3DCore;


//Load the sounds in th GUI interface

public class Manage3DSounds : MonoBehaviour
{
    public Transform soundIcon;
    private AudioClip[] myAudioClips;

    public GameObject mainGraphicPanel;

    public GameObject RecycleBin;

    public GameObject MusicObjGroup;

    private GameObject Sphere;

    private GameObject DraggedIcon = null;
    private GameObject DraggedObj = null;


    void Awake()
    {
        SetIconsfromSoundResources();
    }

    private float distance = 0.0f;

    void FixedUpdate()
    {
        //if we have selected an icon and we drag it into the environment
        if (DraggedIcon)
        {
            //On Drag Object
            DraggedIcon.transform.position = Input.mousePosition;
            
            //If Mouse cursor is within the mainGraphicPanel
            if (mainGraphicPanel.GetComponent<RectTransform>().rect.Contains(DraggedIcon.transform.position))
            {
                //Set Sphere as Active
                Sphere.SetActive(true);

                //For Kinect2.0
                ControlSoundObj(Sphere);

                //Set Icon size as a function of the position in depth of the Sphere
                DraggedIcon.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f) * (Vector3.Distance(GameObject.Find("Scene Camera").transform.position, GameObject.Find("Back").transform.position)) / (2 * Vector3.Distance(GameObject.Find("Scene Camera").transform.position, Sphere.transform.position));

                //Project the Dragged Icomn on the screen 
                Vector3 posViewport = GameObject.Find("Scene Camera").GetComponent<Camera>().WorldToViewportPoint(Sphere.transform.position);
                Vector3 posScreen = GameObject.Find("Main Camera").GetComponent<Camera>().ViewportToScreenPoint(new Vector3(posViewport.x * mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.x / Screen.width, posViewport.y * mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.y / Screen.height, 0.0f));
                DraggedIcon.transform.position = posScreen;
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
    }

    /**********************************************************************/
    // Generic Functions
    /**********************************************************************/
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
            iconObj.SetParent(transform);

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

            float yOffset = iconObj.parent.GetComponent<RectTransform>().sizeDelta.y / 15;
            //Position
            iconObj.GetComponent<RectTransform>().localPosition = new Vector3(iconObj.parent.GetComponent<RectTransform>().sizeDelta.x / 4 * (1 + 2 * (i % 2)), -iconObj.parent.GetComponent<RectTransform>().sizeDelta.y / 8 * (row) + yOffset, 0.0f);

            //Scale
            iconObj.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            /*********************************************************/

            /*********************************************************/
            //Set World Invisible boundaries
            GameObject.Find("Floor").transform.GetChild(0).localPosition = new Vector3(0.0f, GameObject.Find("Sphere").transform.lossyScale.x/2, 0.0f);
            GameObject.Find("Ceiling").transform.GetChild(0).localPosition = new Vector3(0.0f, GameObject.Find("Sphere").transform.lossyScale.x / 2, 0.0f);
            GameObject.Find("Back").transform.GetChild(0).localPosition = new Vector3(0.0f, GameObject.Find("Sphere").transform.lossyScale.x / 2, 0.0f);
            GameObject.Find("Right").transform.GetChild(0).localPosition = new Vector3(0.0f, GameObject.Find("Sphere").transform.lossyScale.x / 2, 0.0f);
            GameObject.Find("Left").transform.GetChild(0).localPosition = new Vector3(0.0f, GameObject.Find("Sphere").transform.lossyScale.x / 2, 0.0f);

            /*********************************************************/
        }
    }

    //Set State of collider of Musical Objects
    void SetAllColliderState(bool state)
    {
        for (int i = 0; i < MusicObjGroup.transform.childCount; i++)
            MusicObjGroup.transform.GetChild(i).GetComponent<Collider>().enabled = state;
    }

    void ControlSoundObj(GameObject obj)
    {
        //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
        Vector2 p = new Vector2((mainGraphicPanel.transform.parent.GetComponent<RectTransform>().sizeDelta.x / Screen.width) * (Input.mousePosition.x - mainGraphicPanel.GetComponent<RectTransform>().position.x) / mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.x,
                                (mainGraphicPanel.transform.parent.GetComponent<RectTransform>().sizeDelta.y / Screen.height) * (Input.mousePosition.y - mainGraphicPanel.GetComponent<RectTransform>().position.y) / mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.y);
        //Clamp the value of p
        p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
        p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);

        //Projection from Ortho Camera (Viewport) to Perspective Camera
        //Ray rayPerspective = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        //For Kinect Manipulation we project from an orthoCamera 
        Ray rayPerspective = GameObject.Find("OrthoCamera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.red);

        //For Kinect
        distance += Input.GetAxis("Mouse ScrollWheel");

        //Clamp Distance
        distance = Mathf.Clamp(distance, GameObject.Find("Front").transform.position.z + obj.transform.lossyScale.z / 2, GameObject.Find("Back").transform.position.z - obj.transform.lossyScale.z / 2);

        //Vector3 vec = GameObject.Find("SceneCamera").GetComponent<Camera>().ViewportToWorldPoint(new Vector3(p.x, p.y, GameObject.Find("Scene Camera").transform.GetComponent<Camera>().nearClipPlane + GameObject.Find("Floor").transform.lossyScale.z * 10 / 2 + distance));
        //For Kinect Manipulation we project from an orthoCamera 
        Vector3 vec = GameObject.Find("OrthoCamera").GetComponent<Camera>().ViewportToWorldPoint(new Vector3(p.x, p.y, GameObject.Find("Scene Camera").transform.GetComponent<Camera>().nearClipPlane + GameObject.Find("Floor").transform.lossyScale.z * 10 / 2 + distance));

        //Clamp vec position
        vec = new Vector3(Mathf.Clamp(vec.x, GameObject.Find("Left").transform.position.x + obj.transform.lossyScale.x / 2, GameObject.Find("Right").transform.position.x - obj.transform.lossyScale.x / 2),
                          Mathf.Clamp(vec.y, GameObject.Find("Floor").transform.position.y + obj.transform.lossyScale.y / 2, GameObject.Find("Ceiling").transform.position.y - obj.transform.lossyScale.y / 2),
                          Mathf.Clamp(vec.z, GameObject.Find("Front").transform.position.z + obj.transform.lossyScale.z / 2, GameObject.Find("Back").transform.position.z - obj.transform.lossyScale.z / 2));

        obj.transform.position = vec;
    }
    /**********************************************************************/
    // Drag Sounds to Scene
    /**********************************************************************/

    public void OnDownObj(GameObject obj)
    {


        //Duplicate the Icon Object
        DraggedIcon = Instantiate<GameObject>(obj);

        //Rename Object
        DraggedIcon.name = obj.name + "bis";
        DraggedIcon.transform.SetParent(obj.transform.parent);

        //Set As Active
        DraggedIcon.SetActive(true);

        //Set degree of transparency
        DraggedIcon.GetComponent<RawImage>().color = new Color(DraggedIcon.GetComponent<RawImage>().color.r, DraggedIcon.GetComponent<RawImage>().color.g, DraggedIcon.GetComponent<RawImage>().color.b, 0.25f);

        //Create Sphere
        Sphere = Instantiate<GameObject>(GameObject.Find("Sphere"));
        Sphere.name = "Sphere" + obj.name;
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
        Destroy(GameObject.Find(obj.name + "bis"));
        //Reset Dragged Icon
        DraggedIcon = null;

        //Activate All Musical Object Collider
        SetAllColliderState(true);

        //If the mouse cursor is outside the mainGraphicPanel when button is up, the sphere is destroyed
        if (!mainGraphicPanel.GetComponent<RectTransform>().rect.Contains(GameObject.Find(obj.name + "bis").transform.position))
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

            //Get the corresponding audio clip - traverse the list of clip
            for (int i = 0; i < myAudioClips.Length; i++)
            {
                //Assign Audio Clip to TBE source
                if (obj.name == myAudioClips[i].name)
                {
                    Sphere.GetComponent<TBE_Source>().clip = myAudioClips[i];
                    Sphere.GetComponent<TBE_Source>().Play();
                    break;
                }
            }
        }
    }


    /**********************************************************************/
    // Drag Sounds inside Scene
    /**********************************************************************/

    public void SelectSoundObj(GameObject myPanel)
    {

        //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
        Vector2 p = new Vector2((myPanel.transform.parent.GetComponent<RectTransform>().sizeDelta.x / Screen.width) * (Input.mousePosition.x - myPanel.GetComponent<RectTransform>().position.x) / myPanel.GetComponent<RectTransform>().sizeDelta.x,
                                (myPanel.transform.parent.GetComponent<RectTransform>().sizeDelta.y / Screen.height) * (Input.mousePosition.y - myPanel.GetComponent<RectTransform>().position.y) / myPanel.GetComponent<RectTransform>().sizeDelta.y);
        //Clamp the value of p
        p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
        p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);


        //Projection from Ortho Camera (Viewport) to Perspective Camera
        Ray rayPerspective = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(rayPerspective, out hit, 100))
        {
            //Only if hit object is a musical object
            if (hit.collider.gameObject.layer == 8)
            {
                DraggedObj = hit.collider.gameObject;
                //Debug.Log("Selected Sound Object: " + DraggedObj.name);

                //Deactivate Collider
                DraggedObj.GetComponent<Collider>().enabled = false;
            }
        }
        
        //Inactivate All Musical Object Collider - So we can place two sound at the same place
        SetAllColliderState(false);
    }

    public void ReleaseSoundObj(GameObject myPanel)
    {
        //Only when the panel contains the Mouse cursor
        if (myPanel.GetComponent<RectTransform>().rect.Contains(Input.mousePosition))
        {
            if (DraggedObj)
            {
                //Reset collider
                DraggedObj.GetComponent<Collider>().enabled = true;

                //Reset Dragged Obj
                DraggedObj = null;
            }
        }

        //Activate All Musical Object Collider 
        SetAllColliderState(true);
    }

    /**********************************************************************/
    // Drag Sounds to Recycle Bin
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
    }
}

