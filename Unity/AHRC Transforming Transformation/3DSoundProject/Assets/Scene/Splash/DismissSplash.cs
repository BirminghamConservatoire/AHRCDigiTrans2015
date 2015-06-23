using UnityEngine;
using System.Collections;

public class DismissSplash : MonoBehaviour {
	public float delayTime = 2;
	IEnumerator Start () { 
		yield return new WaitForSeconds( delayTime );
		Application.LoadLevel( 1 );
	}
}