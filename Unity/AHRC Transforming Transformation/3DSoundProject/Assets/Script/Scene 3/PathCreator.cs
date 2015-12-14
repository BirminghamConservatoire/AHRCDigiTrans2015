using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathCreator : MonoBehaviour {

    private SceneManager mySceneManagerScript;
    private myKinectManager myKinectManagerScript;

    private int id;
    private int mySimpleId;
    private GameObject myPath;

    public Mesh DefaultMesh;

    private List<TrajectoryPtList> myPtList;
    private List<TrajectoryVerticesList> myVtList;
    private LineRenderer lineRenderer;

    private int traj_Acc_ratio = 2;

    private SphereCollider[] mySCArray;

    private int PathId = 0;

    // Use this for initialization
    void OnEnable ()
    {
        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();
        myKinectManagerScript = GameObject.Find("SceneObjects").GetComponent<myKinectManager>();

        //Init new list of point for the new trajectory
        myPtList = new List<TrajectoryPtList>();

        //Init new list of Vertices for the new trajectory
        myVtList = new List<TrajectoryVerticesList>();

        //Create a main path Object
        myPath = new GameObject("path_"+PathId);
        //Debug.Log("New Path Object Created");

        myPath.transform.parent = transform;
        myPath.layer = LayerMask.NameToLayer("Trajectory");//Set on the trajectory layer

        //Create a line Renderer for each object
        lineRenderer = myPath.AddComponent<LineRenderer>();
        lineRenderer.SetWidth(0.075f, 0.075f);

        /********************************************************************************/
        //We aim to obtain a colour Controlled i saturation and light 
        /********************************************************************************/
        Color trajectorycolor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);

        float hue;
        float sat;
        float val;
        float highlight = 1.0f;

        mySceneManagerScript.RGBToHSV(trajectorycolor, out hue, out sat, out val);
        trajectorycolor = mySceneManagerScript.HSVToRGB(hue, Random.Range(0.5f, 1.0f), Random.Range(0.8f, 1.0f));
        /********************************************************************************/

        lineRenderer.SetColors(trajectorycolor, trajectorycolor);
        lineRenderer.material = new Material(Shader.Find("Standard"));
        lineRenderer.material.color = trajectorycolor;

        id = 0;
        mySimpleId = 0;

        PathId++;

    } 
	
	// Update is called once per frame
	void Update ()
    {
        //create nodes of the path
        /*GameObject myPathNode = new GameObject(id.ToString());
        
        //Set New Path as Parent
        myPathNode.transform.parent = myPath.transform;

        //Set the position of the node
        myPathNode.transform.position = myKinectManagerScript.RightHandObj.transform.position;*/

        //Store the node object into the list - Needed for the line rendering
        myPtList.Add(new TrajectoryPtList(myKinectManagerScript.RightHandObj.transform.position));

        //Set the line vertices - Reduce the number of point
        if (id % traj_Acc_ratio == 0)
        {
            lineRenderer.SetVertexCount((int)Mathf.Floor(myPtList.Count / traj_Acc_ratio) +1);

            //clamp the position of the trajectory so the objects following it do not into the walls
            myPtList[id].pos = new Vector3(Mathf.Clamp(myPtList[id].pos.x, GameObject.Find("Left").transform.position.x + 0.5f, GameObject.Find("Right").transform.position.x - 0.5f),
                                           Mathf.Clamp(myPtList[id].pos.y, GameObject.Find("Floor").transform.position.y + 0.5f, GameObject.Find("Ceiling").transform.position.y - 0.5f),
                                           Mathf.Clamp(myPtList[id].pos.z, GameObject.Find("Front").transform.position.z + 0.5f, GameObject.Find("Back").transform.position.z - 0.5f));

            lineRenderer.SetPosition(mySimpleId, myPtList[id].pos);

            //Add Sphere collider to the vertex of the trajectory curve
            SphereCollider myNewCollider = myPath.AddComponent<SphereCollider>();
            myNewCollider.center = myPtList[id].pos;
            myNewCollider.radius = 0.25f;

            //Set the Vertex Postion and collider in rthe vertices list
            myVtList.Add(new TrajectoryVerticesList(mySimpleId, myPtList[id].pos, myNewCollider));

            mySimpleId++;
        }
        
        id++;


    }

    void OnDisable()
    {
            //Attach the Trajetory node to the path

    }


    /*void OnDrawGizmos()
    {
        if (myPtList.Count > 0)
        {
            for (int i = 0; i < myPtList.Count; i++)
            {
                if (myPtList[i] != null && i > 0)
                    Gizmos.DrawLine(myPtList[i - 1].pos, myPtList[i].pos);
            }
        }
    }*/

}
