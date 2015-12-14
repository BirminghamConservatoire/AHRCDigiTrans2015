using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TestTrajectoryWithObj : MonoBehaviour
{

    private SceneManager mySceneManagerScript;

    private myKinectManager myKinectManagerScript;

    public bool HighlightObjWithTrajectory = false;

    public Vector3 closestStartPos;

    public bool releaseOnTrajectory = false;

    public GameObject currentPath;
    public GameObject previousPath;

    public List<ListOfVertices> myList;

    public float speed = 1.0f;
    public float reachDist = 0.0f;
    public int currentPoint = 0;

    void Start()
    {
        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();

        myKinectManagerScript = GameObject.Find("SceneObjects").GetComponent<myKinectManager>();
        
        //Init new list of Vertices for the new trajectory
        myList = new List<ListOfVertices>();

        //Init dumbly previous Path
        previousPath = null;

        //Init currentPath
        currentPath = null;
    }

    void LateUpdate()
    {
        /*if (previousPath == null)
        {
            //Reset current Point when previous path is reseted
            currentPoint = 0;
        }

        if (releaseOnTrajectory && previousPath == currentPath)
        {
            //if hand is release - the object follow the trajectory
            float dist = Vector3.Distance(myList[currentPoint].pos, transform.parent.position);
            //Debug.Log("the current trajectory path for " + transform.parent.name + " is " + currentPath.name);

            transform.parent.position = Vector3.MoveTowards(transform.parent.position, myList[currentPoint].pos, Time.deltaTime * speed);

            if (dist <= reachDist)
            {
                currentPoint++;
            }

            if (currentPoint >= myList.Count)
            {
                currentPoint = myList.Count - 1;
                releaseOnTrajectory = false;

                //Reset dumbly previous Path
                previousPath = null;
            }
        }*/

        if (releaseOnTrajectory)
        {
            //if hand is release - the object follow the trajectory
            float dist = Vector3.Distance(myList[currentPoint].pos, transform.parent.position);
            transform.parent.position = Vector3.MoveTowards(transform.parent.position, myList[currentPoint].pos, Time.deltaTime * speed);

            if (dist <= reachDist)
            {
                currentPoint++;
            }

            if (currentPoint >= myList.Count)
            {
                currentPoint = myList.Count - 1;
                releaseOnTrajectory = false;
            }
        }
        else
            currentPoint = 0;
    }

    void OnTriggerEnter(Collider col)
    {
        //if not drop onto a path (trajectory)
        if (!releaseOnTrajectory)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Trajectory"))
            {
                SphereCollider mySC = (SphereCollider)col;
                //Look for the path to follow
                currentPath = mySC.gameObject;
                closestStartPos = mySC.center;

                //Clean Previous List if there is one
                myList.Clear();

                //Build a list of node for the trajectory
                for (int i = 0; i < currentPath.GetComponents<SphereCollider>().Length; i++)
                    myList.Add(new ListOfVertices(i, currentPath.GetComponents<SphereCollider>()[i].center));
            }
        }
    }

    /*void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Trajectory"))
        {
            SphereCollider mySC = (SphereCollider)col;
            Debug.Log(previousPath);
            currentPath = mySC.gameObject;

            //Only we do it once - not for all the collider that composed the path
            if (currentPath != previousPath)
            {
                Debug.Log("the current trajectory path for " + transform.parent.name + " is " + currentPath.name);

                //Clean Previous List if there is one
                myList.Clear();

                //Build a list of node for the trajectory
                for (int i = 0; i < currentPath.GetComponents<SphereCollider>().Length; i++)
                    myList.Add(new ListOfVertices(i, currentPath.GetComponents<SphereCollider>()[i].center));

                closestStartPos = mySC.center;

                previousPath = currentPath;
            }
            
            HighlightObjWithTrajectory = true;
        } 
    }*/

    void OnTriggerStay(Collider col)
    {
       if (col.gameObject.layer == LayerMask.NameToLayer("Trajectory"))
        {
            HighlightObjWithTrajectory = true;

            /********************************************************************************/
            //Highlight Dragged object when in contact with trajectory
            /********************************************************************************/
            float hue;
            float sat;
            float val;
            float highlight = 1.0f;

            // Highlight the object in contact
            mySceneManagerScript.RGBToHSV(transform.parent.gameObject.GetComponent<Renderer>().material.color, out hue, out sat, out val);
            transform.parent.gameObject.GetComponent<Renderer>().material.color = mySceneManagerScript.HSVToRGB(hue, sat, highlight);
            /********************************************************************************/
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Trajectory"))
        {
            HighlightObjWithTrajectory = false;

            //Update previous path
            //previousPath = currentPath;

            /********************************************************************************/
            //Highlight Dragged object when in contact with trajectory
            /********************************************************************************/
            float hue;
            float sat;
            float val;
            float highlight = 0.5f;
            //Un-Highlight the object when no contact  
            mySceneManagerScript.RGBToHSV(transform.parent.gameObject.GetComponent<Renderer>().material.color, out hue, out sat, out val);
            transform.parent.gameObject.GetComponent<Renderer>().material.color = mySceneManagerScript.HSVToRGB(hue, sat, highlight);
            /********************************************************************************/
        }
    }
}