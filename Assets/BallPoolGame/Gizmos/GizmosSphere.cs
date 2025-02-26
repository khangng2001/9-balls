using UnityEngine;
using System.Collections;

public class GizmosSphere : MonoBehaviour 
{
	void OnDrawGizmosSelected ()
	{
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f * transform.lossyScale.x);
	}
}
