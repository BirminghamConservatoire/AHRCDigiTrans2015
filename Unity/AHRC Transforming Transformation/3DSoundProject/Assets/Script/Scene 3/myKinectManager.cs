using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class myKinectManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    public Body[] GetData()
    {
        return _Data;
    }

    private Vector3 currentLeftHandPos;
    private Vector3 lastLeftHandPos;
    private Vector3 deltaLeftHandPos;

    private Vector3 currentRightHandPos;
    private Vector3 lastRightHandPos;
    private Vector3 deltaRightHandPos;

    void Start()
    {
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
                    else
                        Debug.Log("No tracked data");
                }
                if (idx > -1)
                {
                    if (_Data[idx].HandRightState != HandState.Closed)
                    {
                        Debug.Log("Right Hand Open");

                        currentRightHandPos = new Vector3((float)(_Data[idx].Joints[JointType.HandRight].Position.X * 10.0f),
                                                    (float)(_Data[idx].Joints[JointType.HandRight].Position.Y * 10.0f),
                                                    -(float)(_Data[idx].Joints[JointType.HandRight].Position.Z * 10.0f));

                        deltaRightHandPos = new Vector3(0.0f, 0.0f, 0.0f);

                        lastRightHandPos = currentRightHandPos;
                    }
                    if (_Data[idx].HandRightState == HandState.Closed)
                    {
                        Debug.Log("Right Hand Closed");

                        currentRightHandPos = new Vector3((float)(_Data[idx].Joints[JointType.HandRight].Position.X * 10.0f),
                                                    (float)(_Data[idx].Joints[JointType.HandRight].Position.Y * 10.0f),
                                                    -(float)(_Data[idx].Joints[JointType.HandRight].Position.Z * 10.0f));

                        deltaRightHandPos = currentRightHandPos - lastRightHandPos;

                        lastRightHandPos = currentRightHandPos;

                        //GameObject.Find("rightHand").transform.position += deltaRightHandPos;
                    }

                    if (_Data[idx].HandLeftState != HandState.Closed)
                    {
                        Debug.Log("Left Hand Open");

                        currentLeftHandPos = new Vector3((float)(_Data[idx].Joints[JointType.HandLeft].Position.X * 10.0f),
                                                    (float)(_Data[idx].Joints[JointType.HandLeft].Position.Y * 10.0f),
                                                    -(float)(_Data[idx].Joints[JointType.HandLeft].Position.Z * 10.0f));

                        deltaLeftHandPos = new Vector3(0.0f, 0.0f, 0.0f);

                        lastLeftHandPos = currentLeftHandPos;
                    }
                    if (_Data[idx].HandLeftState == HandState.Closed)
                    {
                        Debug.Log("Left Hand Closed");

                        currentLeftHandPos = new Vector3((float)(_Data[idx].Joints[JointType.HandLeft].Position.X * 10.0f),
                                                    (float)(_Data[idx].Joints[JointType.HandLeft].Position.Y * 10.0f),
                                                    -(float)(_Data[idx].Joints[JointType.HandLeft].Position.Z * 10.0f));

                        deltaLeftHandPos = currentLeftHandPos - lastLeftHandPos;

                        lastLeftHandPos = currentLeftHandPos;

                        //GameObject.Find("leftHand").transform.position += deltaLeftHandPos;
                    }
                }
            }
        }
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
