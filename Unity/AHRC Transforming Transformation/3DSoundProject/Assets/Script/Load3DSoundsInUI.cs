using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TBE_3DCore;


//Load the sounds in th GUI interface

public class Load3DSoundsInUI : MonoBehaviour {

    public Transform soundIcon;
    private AudioClip[] myAudioClips;

    public GameObject mainGraphicPanel;

    private GameObject Sphere; 
    
    
    void Awake()
    {
        LoadAllSoundsFromResources();
    }


    void LoadAllSoundsFromResources()
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
            iconObj.GetComponent<RectTransform>().localPosition = new Vector3(iconObj.parent.GetComponent<RectTransform>().sizeDelta.x / 4 * (1 + 2 * (i % 2)), - iconObj.parent.GetComponent<RectTransform>().sizeDelta.y/8 * (row) + yOffset, 0.0f);

            //Scale
            iconObj.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            /*********************************************************/

            //Assign the ArudioSourceComponent to the iconObj.
            //iconObj.gameObject.AddComponent<TBE_Source>();
            //iconObj.GetComponent<TBE_Source>().clip = myAudioClips[i];
        }       
    }

    void Update()
    {
        //Debug.Log((Screen.width / mainGraphicPanel.transform.parent.GetComponent<RectTransform>().sizeDelta.x));
    }

    public void DragIconObjToScene(GameObject obj)
    {
        //Debug.Log("Drag Icon Object");

        //On Drag Object
        GameObject.Find(obj.name + "bis").transform.position = Input.mousePosition;


        if (mainGraphicPanel.GetComponent<RectTransform>().rect.Contains(GameObject.Find(obj.name + "bis").transform.position))
        {
            //Debug.Log("Icon Dragged in Main Graphic Panel");

            //Projection from Screen to Ortho camera - p return a coordinates float values for x and y between 0 and 1
            /*Vector2 p = GameObject.Find("Main Camera").GetComponent<Camera>().ScreenToViewportPoint(new Vector3(
                                                                                                   Input.mousePosition.x,
                                                                                                   Input.mousePosition.y,
                                                                                                   Input.mousePosition.z));*/

            //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
            //Debug.Log("x " + (mainGraphicPanel.transform.parent.GetComponent<RectTransform>().sizeDelta.x/Screen.width) * (Input.mousePosition.x - mainGraphicPanel.GetComponent<RectTransform>().position.x) / mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.x);
            //Debug.Log("y " + (mainGraphicPanel.transform.parent.GetComponent<RectTransform>().sizeDelta.y/Screen.height) * (Input.mousePosition.y - mainGraphicPanel.GetComponent<RectTransform>().position.y) / mainGraphicPanel.GetComponent<RectTransform>().sizeDelta.y);
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
                 Sphere.transform.position = rayPerspective.origin + rayPerspective.direction * hit.distance/2;

        }

        
    }

    public void OnDownObj(GameObject obj)
    {
        //Debug.Log("Down Icon Object");

        //Duplicate the Icon Object
        GameObject go = Instantiate<GameObject>(obj);
        go.name = obj.name + "bis";
        go.transform.SetParent(obj.transform.parent);
        //Set degree of transparency
        go.GetComponent<RawImage>().color = new Color(go.GetComponent<RawImage>().color.r, go.GetComponent<RawImage>().color.g, go.GetComponent<RawImage>().color.b, 0.25f);

        //Create Sphere
        Sphere = Instantiate<GameObject>(GameObject.Find("Sphere"));
        Sphere.name = "Sphere" + obj.name;
    }

    public void OnUpObj(GameObject obj)
    {
        //Debug.Log("Up Icon Object");

        //Delete the Icon Obj
        Destroy(GameObject.Find(obj.name + "bis"));
    }
}
