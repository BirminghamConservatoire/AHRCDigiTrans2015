using UnityEngine;
using System.Collections;
using Windows.Kinect;
using UnityEngine.UI;
using TBE_3DCore;

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

    public Texture HandFull;
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

    private bool leftHandVolumeCtrl = false;
    private Vector3 currentLeftHandPos;
    private Vector3 lastLeftHandPos;
    private Vector3 deltaLeftHandPos;

    public bool previousFingerHandUpState = false;
    public bool createNewPath = false;

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

                        /********************************************************/
                        //Hand Closed
                        /********************************************************/
                        if (_Data[idx].HandRightState == HandState.Closed)
                        {
                            RightHandIcon.GetComponent<RawImage>().texture = HandFist;
                            RightHandObj.GetComponent<Renderer>().material.mainTexture = HandFist;
                            hasBeenSelectedbyHand = true;

                            if (prevRightHandOpenState)
                                selectionStateMachine = 1; //Selection
                            else
                                selectionStateMachine = 0;//No change

                            prevRightHandOpenState = false;

                            //Reset finger State
                            previousFingerHandUpState = false;
                            GameObject.Find("SceneObjects").GetComponent<PathCreator>().enabled = false;
                        }

                        /********************************************************/
                        //Only one finger up
                        /********************************************************/
                        if ( _Data[idx].HandRightState == HandState.Lasso)
                        {
                            RightHandIcon.GetComponent<RawImage>().texture = HandFinger;
                            RightHandObj.GetComponent<Renderer>().material.mainTexture = HandFinger;

                            //Check previous state of finger - if it is the first time we have the finger up
                            if (!previousFingerHandUpState)
                            {
                                //Set parameters to create new Path
                                createNewPath = true;
                                previousFingerHandUpState = true;

                                GameObject.Find("SceneObjects").GetComponent<PathCreator>().enabled = true;
                            }
                            else
                                createNewPath = false;
                        }

                        /********************************************************/
                        //Hand fully opened
                        /********************************************************/
                        if (_Data[idx].HandRightState == HandState.Open)
                        {
                            RightHandIcon.GetComponent<RawImage>().texture = HandFull;
                            RightHandObj.GetComponent<Renderer>().material.mainTexture = HandFull;
                            hasBeenSelectedbyHand = false;
                            
                            if (!prevRightHandOpenState)
                                selectionStateMachine = -1; //De-Selection
                            else
                                selectionStateMachine = 0;//No change

                            prevRightHandOpenState = true;

                            //Reset finger State
                            previousFingerHandUpState = false;
                            GameObject.Find("SceneObjects").GetComponent<PathCreator>().enabled = false;
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
                        /*currentRightHandPos = new Vector3((float)(_Data[idx].Joints[JointType.HandRight].Position.X * 10.0f),
                                                        (float)(_Data[idx].Joints[JointType.HandRight].Position.Y * 10.0f),
                                                        -(float)(_Data[idx].Joints[JointType.HandRight].Position.Z * 10.0f));*/
                        currentRightHandPos = new Vector3((float)((_Data[idx].Joints[JointType.HandRight].Position.X + KinectOriginOffset.x) * KinectWorkSpaceScale.x),
                                                        (float)((_Data[idx].Joints[JointType.HandRight].Position.Y + KinectOriginOffset.y) * KinectWorkSpaceScale.y),
                                                        (float)((-(_Data[idx].Joints[JointType.HandRight].Position.Z) + KinectOriginOffset.z) * KinectWorkSpaceScale.z));//We offset the Z axis otherwise the origin is the kinect itself
                        deltaRightHandPos = new Vector3(0.0f,0.0f,0.0f);
                        lastRightHandPos = currentRightHandPos;

                        //Reset finger State
                        previousFingerHandUpState = false;
                        GameObject.Find("SceneObjects").GetComponent<PathCreator>().enabled = false;
                    }


                    if (_Data[idx].HandLeftState != HandState.NotTracked)
                    {
                        //Manage Left Hand Position when tracked

                        //Manage Volume Control
                        if (_Data[idx].HandLeftState == HandState.Closed)
                        {
                            //Debug.Log("Closed");
                            leftHandVolumeCtrl = false;
                        }
                        if (_Data[idx].HandLeftState == HandState.Open)
                        {
                            //Debug.Log("Open");
                            leftHandVolumeCtrl = false;
                        }
                        if (_Data[idx].HandLeftState == HandState.Lasso)
                        {
                            //Debug.Log("Lasso");
                            leftHandVolumeCtrl = true;
                        }

                        currentLeftHandPos = new Vector3((float)((_Data[idx].Joints[JointType.HandLeft].Position.X + KinectOriginOffset.x) * KinectWorkSpaceScale.x),
                                                        (float)((_Data[idx].Joints[JointType.HandLeft].Position.Y + KinectOriginOffset.y) * KinectWorkSpaceScale.y),
                                                        (float)((-(_Data[idx].Joints[JointType.HandLeft].Position.Z) + KinectOriginOffset.z) * KinectWorkSpaceScale.z));//We offset the Z axis otherwise the origin is the kinect itself
                        deltaLeftHandPos = currentLeftHandPos - lastLeftHandPos;
                        lastLeftHandPos = currentLeftHandPos;

                    }

                    else 
                    {
                        //Manage Left Hand Position when not tracked
                        currentLeftHandPos = new Vector3((float)((_Data[idx].Joints[JointType.HandLeft].Position.X + KinectOriginOffset.x) * KinectWorkSpaceScale.x),
                                                        (float)((_Data[idx].Joints[JointType.HandLeft].Position.Y + KinectOriginOffset.y) * KinectWorkSpaceScale.y),
                                                        (float)((-(_Data[idx].Joints[JointType.HandLeft].Position.Z) + KinectOriginOffset.z) * KinectWorkSpaceScale.z));//We offset the Z axis otherwise the origin is the kinect itself
                        deltaLeftHandPos = new Vector3(0.0f, 0.0f, 0.0f);
                        lastLeftHandPos = currentLeftHandPos;
                    }
                }
            }
        }

        //Project Kinect hand to the screen as Icon
        ProjectControlObjectOntoScreen(RightHandIcon, RightHandObj, currentRightHandPos);

        //Control the volume of Dragged Object
        if (mySceneManagerScript.DraggedObj)
        {
            if (leftHandVolumeCtrl)
            {
                //Control Volume
                mySceneManagerScript.ControlObjVolume(mySceneManagerScript.DraggedObj, deltaLeftHandPos.y / 5.0f);

                //Manage Interface to control volume
                mySceneManagerScript.ManageVolumeInterface(true, mySceneManagerScript.DraggedObj);
            }
            else //Hide Interface to control volume
                mySceneManagerScript.ManageVolumeInterface(false, mySceneManagerScript.DraggedObj);
        }
        else //Hide Interface to control volume
            mySceneManagerScript.ManageVolumeInterface(false, null);   
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

        /*****************************************************************************************************************************************************/
        //When the hand obj is on boundary of the VE - the hand icon and hand object will be detached (no more superposition) - The hand object is thus darkened
        /*****************************************************************************************************************************************************/

        if (vec.x == GameObject.Find("Left").transform.position.x + obj.transform.lossyScale.x / 2 || vec.x == GameObject.Find("Right").transform.position.x - obj.transform.lossyScale.x / 2
        || vec.y == GameObject.Find("Floor").transform.position.y + obj.transform.lossyScale.y / 2 || vec.y == GameObject.Find("Ceiling").transform.position.y - obj.transform.lossyScale.y / 2
        || vec.z == GameObject.Find("Front").transform.position.z + obj.transform.lossyScale.z / 2 || vec.z == GameObject.Find("Back").transform.position.z - obj.transform.lossyScale.z / 2)
            RightHandObj.GetComponent<Light>().enabled = false;
        else
            RightHandObj.GetComponent<Light>().enabled = true;

        /*****************************************************************************************************************************************************/

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
