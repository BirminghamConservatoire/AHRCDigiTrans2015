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

    private GameObject Sphere;

    private GameObject DraggedObj = null;

    void Awake()
    {
        SetIconsfromSoundResources();
    }


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
        }
    }

    //Update has a lower priority order than Other event - see docs.unity3d.com/Manual/ExecutionOrder.html
    void Update()
    {
        //we proceed in such a way otherwise there are some conflict between the priority of the event
        if (!mainGraphicPanel.GetComponent<RectTransform>().rect.Contains(Input.mousePosition))
        {
            if (Input.GetMouseButtonUp(0))
            {
                    //Debug.Log("Pressed left click Outside main window");
                    //Release the Dragged Object
                    if (DraggedObj)
                    {
                        Debug.Log("Release Dragged Object");
                        //Reset collider
                        DraggedObj.GetComponent<Collider>().enabled = true;

                        //Reset Dragged Obj
                        DraggedObj = null;
                    }
            }
        }  
    }


    /**********************************************************************/
    // Drag Sounds to Scene
    /**********************************************************************/
    public void DragIconObjToScene(GameObject obj)
    {
        //On Drag Object
        GameObject.Find(obj.name + "bis").transform.position = Input.mousePosition;

        //If Mouse cursor is within the mainGraphicPanel
        if (mainGraphicPanel.GetComponent<RectTransform>().rect.Contains(GameObject.Find(obj.name + "bis").transform.position))
        {
            //Set Sphere as Active
            Sphere.SetActive(true);

            //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
            Vector2 p = new Vector2((mainGraphicPanel.transform.parent.GetComponent<RectTransform>().sizeDelta.x / Screen.width) * (Input.mousePosition.x - mainGraphicPanel.GetComponent<RectTransform>().position.x) / mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.x,
                                    (mainGraphicPanel.transform.parent.GetComponent<RectTransform>().sizeDelta.y / Screen.height) * (Input.mousePosition.y - mainGraphicPanel.GetComponent<RectTransform>().position.y) / mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.y);
            //Clamp the value of p
            p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
            p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);


            //Projection from Ortho Camera (Viewport) to Perspective Camera
            Ray rayPerspective = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
            Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.green);

            RaycastHit hit;
            if (Physics.Raycast(rayPerspective, out hit, 100))
            {
                //Check the Distance between the center of the ball and the wall in contact
                Ray myRay = new Ray(Sphere.transform.position, -hit.transform.up);
                //Debug.DrawRay(myRay.origin, myRay.direction * 10, Color.blue);

                //Just in case offset the distance
                float offset = 0.0f;

                Sphere.transform.position = rayPerspective.origin + rayPerspective.direction * (hit.distance);

                RaycastHit myHit;
                if (Physics.Raycast(myRay, out myHit, Sphere.transform.lossyScale.x / 1.99f))//it used to be 2.0f but the positon was jaggy
                    offset = Sphere.transform.lossyScale.x / 2 / (Mathf.Cos(Vector3.Angle(-rayPerspective.direction, hit.transform.up) * Mathf.Deg2Rad));
                
                //Offset the distance so that the sphere is always appearing inside the room
                Sphere.transform.position = rayPerspective.origin + rayPerspective.direction * (hit.distance - offset);

                //Set Front limit to the workspace
                if (Sphere.transform.position.z < -4.5f)
                    Sphere.transform.position = new Vector3(Sphere.transform.position.x, Sphere.transform.position.y, -1 * GameObject.Find("Floor").transform.localScale.z * 10 / 2 + Sphere.transform.lossyScale.z / 2);//Set the edge of the floor as the limit in depth
            }
                

        }

        // the mouse cursor is located outside the mainGraphicPanel
        else
        {
            //Set Sphere as Inactive
            Sphere.SetActive(false);
        }
    }

    public void OnDownObj(GameObject obj)
    {
        //Duplicate the Icon Object
        GameObject go = Instantiate<GameObject>(obj);

        //Rename Object
        go.name = obj.name + "bis";
        go.transform.SetParent(obj.transform.parent);

        //Set As Active
        go.SetActive(true);

        //Set degree of transparency
        go.GetComponent<RawImage>().color = new Color(go.GetComponent<RawImage>().color.r, go.GetComponent<RawImage>().color.g, go.GetComponent<RawImage>().color.b, 0.25f);

        //Create Sphere
        Sphere = Instantiate<GameObject>(GameObject.Find("Sphere"));
        Sphere.name = "Sphere" + obj.name;
    }


    public void OnUpObj(GameObject obj)
    {
        //Delete the Icon Obj
        Destroy(GameObject.Find(obj.name + "bis"));

        //If the mouse cursor is outside the mainGraphicPanel when button is up, the sphere is destroyed
        if (!mainGraphicPanel.GetComponent<RectTransform>().rect.Contains(GameObject.Find(obj.name + "bis").transform.position))
            Destroy(Sphere);
        else // The Sphere is naturally placed in the environment
        {
            //Set obj in Hierarchy
            Sphere.transform.parent = GameObject.Find("MusicalObjects").transform;

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

                    //Deactivate Collider
                    DraggedObj.GetComponent<Collider>().enabled = false;
                }
            }
            
    }


    public void ReleaseSoundObj(GameObject myPanel)
    {
        //Only whenthe panel contains the Mouse cursor
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
    }

    public void DragSoundObj(GameObject myPanel)
    {
        if(DraggedObj)
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

                //Check the Distance between the center of the ball and the wall in contact
                Ray myRay = new Ray(Sphere.transform.position, -hit.transform.up);
                //Debug.DrawRay(myRay.origin, myRay.direction * 10, Color.blue);

                //Just in case offset the distance
                float offset = 0.0f;

                DraggedObj.transform.position = rayPerspective.origin + rayPerspective.direction * (hit.distance);

                RaycastHit myHit;
                if (Physics.Raycast(myRay, out myHit, Sphere.transform.lossyScale.x / 1.99f))//it used to be 2.0f but the positon was jaggy
                    offset = Sphere.transform.lossyScale.x /2 / (Mathf.Cos(Vector3.Angle(-rayPerspective.direction, hit.transform.up) * Mathf.Deg2Rad));

                //Offset the distance so that the sphere is always appearing inside the room
                DraggedObj.transform.position = rayPerspective.origin + rayPerspective.direction * (hit.distance - offset);

                //Set Front limit to the workspace
                if (DraggedObj.transform.position.z < -4.5f)
                    DraggedObj.transform.position = new Vector3(DraggedObj.transform.position.x,DraggedObj.transform.position.y,-1 * GameObject.Find("Floor").transform.localScale.z*10/2 + Sphere.transform.lossyScale.z/2);//Set the edge of the floor as the limit in depth

            }
        }
    }


    /**********************************************************************/
    // Drag Sounds to Recycle Bin
    /**********************************************************************/
    public void RecycleSound(GameObject bin)
    {
        if(bin.GetComponent<RectTransform>().rect.Contains(Input.mousePosition))
            Debug.Log(DraggedObj);
        if (DraggedObj)
        {
            //Debug.Log("Recycle Object");
            //Destroy Dragged Object
            Destroy(DraggedObj);
            
            //Reset DraggedObj
            DraggedObj = null;
        }
    }
}
