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

    private List<TrajectoryVerticesList> myPtList;
    private LineRenderer lineRenderer;

    private int traj_Acc_ratio = 2;



    // Use this for initialization
    void OnEnable ()
    {
        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();
        myKinectManagerScript = GameObject.Find("SceneObjects").GetComponent<myKinectManager>();

        //Init new list of point for the new trajectory
        myPtList = new List<TrajectoryVerticesList>();

        //Create a main path Object
        myPath = new GameObject("path");
        //Debug.Log("New Path Object Created");

        myPath.transform.parent = transform;
        myPath.layer = LayerMask.NameToLayer("Trajectory");//Set on the trajectory layer

        //Create a line Renderer for each object
        lineRenderer = myPath.AddComponent<LineRenderer>();
        lineRenderer.SetWidth(0.075f, 0.075f);
        Color trajectorycolor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);
        lineRenderer.SetColors(trajectorycolor, trajectorycolor);
        lineRenderer.material = new Material(Shader.Find("Standard"));
        lineRenderer.material.color = trajectorycolor;

        id = 0;
        mySimpleId = 0;

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

        //Store the node object into the list
        myPtList.Add(new TrajectoryVerticesList(myKinectManagerScript.RightHandObj.transform.position));
        
        //Set the line vertices - Reduce the number of point
        if (id % traj_Acc_ratio == 0)
        {
            lineRenderer.SetVertexCount((int)Mathf.Floor(myPtList.Count / traj_Acc_ratio) +1);
            lineRenderer.SetPosition(mySimpleId, myPtList[id].pos);
            mySimpleId++;
        }
        
        id++;


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
