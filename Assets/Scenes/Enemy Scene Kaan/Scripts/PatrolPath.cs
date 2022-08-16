using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    [SerializeField] float wayPointRadius;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int i = 0; i < transform.childCount; i++)
        {
            int j = GetNextIndex(i);
            Gizmos.DrawSphere(GetWayPointPosition(i).position, wayPointRadius);
            Gizmos.DrawLine(GetWayPointPosition(i).position, GetWayPointPosition(j).position);
        }
    }

    //Abstractions
    public int GetNextIndex(int i)
    {
        if (i + 1 == transform.childCount)
        {
            return 0;
        }
        return i + 1;
    }

    public Transform GetWayPointPosition(int i)
    {
        return transform.GetChild(i);
    }
}
