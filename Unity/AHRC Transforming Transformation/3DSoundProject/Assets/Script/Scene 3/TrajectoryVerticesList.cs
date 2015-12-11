using UnityEngine;
using System.Collections;
using System; //This allows the IComparable Interface

//This is a list of Empty GameObject that serve as vertices of the trajectory 
public class TrajectoryVerticesList 
    {
        public Vector3 pos;

        public TrajectoryVerticesList(Vector3 newPos)
        {
            pos = newPos;
        }
    }

