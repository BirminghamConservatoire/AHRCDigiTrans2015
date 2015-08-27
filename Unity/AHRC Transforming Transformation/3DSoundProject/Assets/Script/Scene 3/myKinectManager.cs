using UnityEngine;
using System.Collections;
using Windows.Kinect;
using UnityEngine.UI;

public class myKinectManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    public Body[] GetData()
    {
        return _Data;
    }

    private Vector3 currentRightHandPos;
    private Vector3 lastRightHandPos;
    private Vector3 deltaRightHandPos;

    public GameObject RightHandObj;
    public GameObject RightHandIcon;

    public Texture HandFinger;
    public Texture HandFist;

    public float distance = 0.0f;
    public Vector3 KinectOriginOffset;

    public Vector3 KinectWorkSpaceScale;

    private SceneManager mySceneManagerScript;

    //selectionStateMachine is a state machine 1 means selection/0 no change from previous state/-1 deselection 
    public float selectionStateMachine = 0;

    public bool hasBeenSelectedbyHand = false;

    private bool prevRightHandOpenState = false;

    void Start()
    {

        mySceneManagerScript = GameObject.Find("SceneObjects").GetComponent<SceneManager>();

        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(_Data);

                frame.Dispose();
                frame = null;



                int idx = -1;
                for (int i = 0; i < _Sensor.BodyFrameSource.BodyCount; i++)
                {
                    if (_Data[i].IsTracked)
                    {
                        idx = i;
                    }
                }
                if (idx > -1)
                {

                    if (_Data[idx].HandRightState != HandState.NotTracked)
                    {
                        //Manage Cursor state
                        if (_Data[idx].HandRightState == HandState.Closed)
                        {
                            //Debug.Log("Right Hand Open");
                            RightHandIcon.GetComponent<RawImage>().texture = HandFist;
                            hasBeenSelectedbyHand = true;

                            if (prevRightHandOpenState)
                                selectionStateMachine = 1; //Selection
                            else
                                selectionStateMachine = 0;//No change

                            prevRightHandOpenState = false;
                        }
                        if (_Data[idx].HandRightState == HandState.Open)
                        {
                            //Debug.Log("Right Hand Closed");
                            RightHandIcon.GetComponent<RawImage>().texture = HandFinger;
                            hasBeenSelectedbyHand = false;
                            
                            if (!prevRightHandOpenState)
                                selectionStateMachine = -1; //De-Selection
                            else
                                selectionStateMachine = 0;//No change

                            prevRightHandOpenState = true;
                        }

                        //Debug.Log("State Machine " + selectionStateMachine);


                        //Manage Right Hand Position when tracked
                        currentRightHandPos = new Vector3((float)((_Data[idx].Joints[JointType.HandRight].Position.X + KinectOriginOffset.x) * KinectWorkSpaceScale.x),
                                                        (float)((_Data[idx].Joints[JointType.HandRight].Position.Y + KinectOriginOffset.y) * KinectWorkSpaceScale.y),
                                                        (float)((-(_Data[idx].Joints[JointType.HandRight].Position.Z) + KinectOriginOffset.z) * KinectWorkSpaceScale.z));//We offset the Z axis otherwise the origin is the kinect itself
                        deltaRightHandPos = currentRightHandPos - lastRightHandPos;
                        lastRightHandPos = currentRightHandPos;

                        /************************************************************************************************/
                        //Manage the depth of the KINECT hand in the environment PS: currentRightHandPos is relative to the Kinect
                        distance = currentRightHandPos.z;

                        //Clamp Distance
                        distance = Mathf.Clamp(distance, GameObject.Find("Front").transform.position.z + mySceneManagerScript.SpherePattern.transform.lossyScale.z / 2, GameObject.Find("Back").transform.position.z - mySceneManagerScript.SpherePattern.transform.lossyScale.z / 2);
                        /************************************************************************************************/
                    }
                    else
                    {
                        //Manage Right Hand Position when not tracked
                        currentRightHandPos = new Vector3((float)(_Data[idx].Joints[JointType.HandRight].Position.X * 10.0f),
                                                        (float)(_Data[idx].Joints[JointType.HandRight].Position.Y * 10.0f),
                                                        -(float)(_Data[idx].Joints[JointType.HandRight].Position.Z * 10.0f));
                        deltaRightHandPos = new Vector3(0.0f,0.0f,0.0f);
                        lastRightHandPos = currentRightHandPos;
                    }
                }
            }
        }

        //Project Kinect hand to the screen as Icon
        ProjectControlObjectOntoScreen(RightHandIcon, RightHandObj, currentRightHandPos);
        
    }


    public void ProjectControlObjectOntoScreen(GameObject projectedIcon, GameObject obj, Vector3 KinectHandPos)
    {
        
        //Offset on X axis - we pffset the position of the hand compared to the Kinect 
        float offset = Screen.width / 3.0f;

        //Project the Hand Icon from the Rw tracked by Kinect to Viewport
        Vector3 posViewport = GameObject.Find("OrthoCamera").GetComponent<Camera>().WorldToViewportPoint(KinectHandPos);
        //Project the Hand Icon from the  Viewport to Screen
        Vector3 posScreen = GameObject.Find("OrthoCamera").GetComponent<Camera>().ViewportToScreenPoint(new Vector3(posViewport.x, posViewport.y, 0.0f));
        
        //Offset on X
        posScreen.x -= offset;
        
        //Assign To the Icon
        projectedIcon.transform.position = posScreen;

        //Projection of the icon position from Screen into the Real world as hand object
        ControlObjWithKinect(projectedIcon, obj);
    }

    public void ControlObjWithKinect( GameObject Icon, GameObject obj)
    {
        //Projection from RT to Viewport - p return a coordinates float values for x and y between 0 and 1
        Vector2 p = new Vector2((mySceneManagerScript.mainGraphicWindow.transform.parent.GetComponent<RectTransform>().rect.width / Screen.width) * (Icon.transform.position.x - mySceneManagerScript.mainGraphicWindow.GetComponent<RectTransform>().position.x) / mySceneManagerScript.mainGraphicWindow.GetComponent<RectTransform>().rect.width,
                                (mySceneManagerScript.mainGraphicWindow.transform.parent.GetComponent<RectTransform>().rect.height / Screen.height) * (Icon.transform.position.y - mySceneManagerScript.mainGraphicWindow.GetComponent<RectTransform>().position.y) / mySceneManagerScript.mainGraphicWindow.GetComponent<RectTransform>().rect.height);

        //Clamp the value of p
        p.x = Mathf.Clamp(p.x, 0.0f, 1.0f);
        p.y = Mathf.Clamp(p.y, 0.0f, 1.0f);

        //Projection from Ortho Camera (Viewport) to Perspective Camera
        Ray rayPerspective = GameObject.Find("OrthoCamera").GetComponent<Camera>().ViewportPointToRay(new Vector3(p.x, p.y, 0.0f));
        //Debug.DrawRay(rayPerspective.origin, rayPerspective.direction * 10, Color.blue);

        //For Kinect Manipulation
        Vector3 vec = GameObject.Find("Scene Camera").GetComponent<Camera>().ViewportToWorldPoint(new Vector3(p.x, p.y, GameObject.Find("Scene Camera").transform.GetComponent<Camera>().nearClipPlane + GameObject.Find("Floor").transform.lossyScale.z * 10 / 2 + distance));

        //Clamp vec position
        vec = new Vector3(Mathf.Clamp(vec.x, GameObject.Find("Left").transform.position.x + obj.transform.lossyScale.x / 2, GameObject.Find("Right").transform.position.x - obj.transform.lossyScale.x / 2),
                          Mathf.Clamp(vec.y, GameObject.Find("Floor").transform.position.y + obj.transform.lossyScale.y / 2, GameObject.Find("Ceiling").transform.position.y - obj.transform.lossyScale.y / 2),
                          Mathf.Clamp(vec.z, GameObject.Find("Front").transform.position.z + obj.transform.lossyScale.z / 2, GameObject.Find("Back").transform.position.z - obj.transform.lossyScale.z / 2));

        obj.transform.position = vec;

        //Set Icon size as a function of the position in depth of the Sphere
        Icon.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f) * (Vector3.Distance(GameObject.Find("Scene Camera").transform.position, GameObject.Find("Back").transform.position)) / (2 * Vector3.Distance(GameObject.Find("Scene Camera").transform.position, obj.transform.position));


    }


    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
