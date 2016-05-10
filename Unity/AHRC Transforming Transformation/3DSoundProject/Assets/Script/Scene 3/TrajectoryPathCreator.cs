using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrajectoryPathCreator : MonoBehaviour
{
    public GameObject trailPrefab;
    public SplineTrailRenderer trailReference;
    SplineTrailRenderer currentTrail = null;

    private SceneManager mySceneManagerScript;
    private myKinectManager myKinectManagerScript;

    void OnEnable()
    {
        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();
        myKinectManagerScript = GameObject.Find("SceneObjects").GetComponent<myKinectManager>();

        currentTrail = (Instantiate(trailPrefab) as GameObject).GetComponent<SplineTrailRenderer>();
        trailReference.ImitateTrail(currentTrail);
    }

    void Update()
    {
        currentTrail.transform.position = myKinectManagerScript.RightHandObj.transform.position;
        trailReference.Clear();
    }

}