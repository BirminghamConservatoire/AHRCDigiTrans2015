using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneratePath : MonoBehaviour 
{
	public List<Transform> pointList;
	
	void Start() 
	{
		SplineTrailRenderer trail = GetComponent<SplineTrailRenderer>();
		
		trail.Clear();
		trail.spline.Clear();
		
		foreach(Transform t in pointList)
		{
			trail.spline.knots.Add(new Knot(t.position));
		}
		
		trail.spline.Parametrize();
	}

}
