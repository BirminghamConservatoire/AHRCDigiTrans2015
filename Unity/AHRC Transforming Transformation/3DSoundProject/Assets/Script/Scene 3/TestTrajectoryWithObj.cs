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

        //Init currentPath
        currentPath = null;
    }

    private bool forward = true;

    void LateUpdate()
    {
        if (releaseOnTrajectory)
        {
            //if hand is release - the object follow the trajectory
            float dist = Vector3.Distance(myList[currentPoint].pos, transform.parent.position);
            transform.parent.position = Vector3.MoveTowards(transform.parent.position, myList[currentPoint].pos, Time.deltaTime * speed);

            if (dist <= reachDist)
            {
                //Mechanism to manage the forward and backward following of the trajectory
                if (forward)
                    currentPoint++;
                else
                    currentPoint--;
            }

            //Safety Mechanism
            currentPoint = Mathf.Clamp(currentPoint, 0, myList.Count - 1);

            if (currentPoint >= myList.Count - 1 || currentPoint <= 0)
            {
                //Manage the Loop of the trajectory
                forward = !forward;
            }

            HighlightObjWithTrajectory = true;
            //Highlight Object on trajectory - Overwrite the highlight on the Event functions
            HighlightObjOnPath(1.0f);
        }
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
                {
                    myList.Add(new ListOfVertices(i, currentPath.GetComponents<SphereCollider>()[i].center));

                    //Get the current point of insertion onto the path - set the current point
                    if (currentPath.GetComponents<SphereCollider>()[i].center == closestStartPos)
                    {
                        //Set the current point of insertion on the trajectory
                        currentPoint = i;
                        //Debug.Log("currentPoint is " + currentPoint);
                    }
                }
            }
            
        }
    }


    void OnTriggerStay(Collider col)
    {
       if (col.gameObject.layer == LayerMask.NameToLayer("Trajectory"))
        {
            HighlightObjWithTrajectory = true;

            /********************************************************************************/
            //Highlight Dragged object when in contact with trajectory
            /********************************************************************************/
            HighlightObjOnPath(1.0f);
            /********************************************************************************/
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Trajectory"))
        {
            HighlightObjWithTrajectory = false;

            /********************************************************************************/
            //Highlight Dragged object when in contact with trajectory
            /********************************************************************************/
            HighlightObjOnPath(0.5f);
            /********************************************************************************/
        }
    }

    void HighlightObjOnPath(float highlight)
    {
        /********************************************************************************/
        //Highlight Dragged object when in contact with trajectory
        /********************************************************************************/
        float hue;
        float sat;
        float val;

        // Highlight the object in contact
        mySceneManagerScript.RGBToHSV(transform.parent.gameObject.GetComponent<Renderer>().material.color, out hue, out sat, out val);
        transform.parent.gameObject.GetComponent<Renderer>().material.color = mySceneManagerScript.HSVToRGB(hue, sat, highlight);
        /********************************************************************************/
    }
}