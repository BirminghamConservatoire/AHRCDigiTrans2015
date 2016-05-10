using UnityEngine;
using System.Collections;

public class SendOSCMessage : MonoBehaviour {


    public UniOSC.UniOSCEventDispatcherButton objectEvent;

    void OnCollisionEnter() {
        objectEvent.SendOSCMessageDown();
    }

    void OnCollisionExit() {

        objectEvent.SendOSCMessageUp();
    }
}
