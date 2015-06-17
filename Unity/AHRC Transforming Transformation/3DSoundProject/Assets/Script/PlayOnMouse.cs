using UnityEngine;
using System.Collections;
using TBE_3DCore;

public class PlayOnMouse : MonoBehaviour {

    void OnMouseDown()
    {
         transform.GetComponent<TBE_Source>().Play();
    }
}
