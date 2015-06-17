using UnityEngine;
using System.Collections;
using TBE_3DCore;

public class PlaySoundOnCollider : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform.name.Contains("Ball"))
        {
            transform.GetComponent<TBE_Source>().Play();
        }
    }
}
