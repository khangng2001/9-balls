using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;

namespace BallPool.Mechanics
{
    public class PocketListener : MonoBehaviour
    {
        public int id;
        [SerializeField] private Transform pocketTarget;
        public Vector3 target{ get{ return pocketTarget.position; }}
    }
}
