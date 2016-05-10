using UnityEngine;
using System.Collections;
using System; //This allows the IComparable Interface

public class TrajectoryVerticesList
{
    public int Id;
    public Vector3 pos;
    public SphereCollider sc;

    public TrajectoryVerticesList(int newId, Vector3 newPos, SphereCollider newSc)
    {
        Id = newId;
        pos = newPos;
        sc = newSc;
    }
}
