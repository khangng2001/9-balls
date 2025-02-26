using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallPool.Mechanics;

namespace BallPool
{
    public class BallListener : MonoBehaviour
    {
        public int id;
        public Rigidbody body;

        public PocketListener pocket{ get; set; }

        public PhysicsManager physicsManager;

        public float radius{ get; private set; }

        public int pocketId{ get; set; }

        public int hitShapeId{ get; set; }

        public Vector3 normalizedVelocity{ get { return body.linearVelocity / physicsManager.ballMaxVelocity; } }

        private bool firstHit = false;
        private bool inMove;

        void OnCollisionEnter(Collision other)
        {
            if (physicsManager.inMove)
            {
                firstHit = true;
            }
            if (!firstHit && other.gameObject.layer == LayerMask.NameToLayer("Cloth"))
            {
                firstHit = true;
                if (id != 0)
                {
                    body.Sleep();
                }
            }
        }

      

        public void OnTriggerEnter(Collider other)
        {
            if (BallPoolGameLogic.playMode == PlayMode.Replay || BallPoolGameLogic.controlFromNetwork)
            {
                return;
            }
            PocketListener pocket = other.GetComponent<PocketListener>();
            if (pocket)
            {
                OnEnterPocket(pocket);
            }
        }

        public void OnEnterPocket(PocketListener pocket)
        {
            if (!body.isKinematic)
            {
                body.isKinematic = true;
                pocketId = pocket.id;
                hitShapeId = -2;
                Debug.Log(id + " OnEnterPocket");
                physicsManager.CallOnBallHitPocket(this, pocket, true);
            }
        }

        public void OnCollisionExit(Collision collision)
        {
            if (BallPoolGameLogic.playMode == PlayMode.Replay || BallPoolGameLogic.controlFromNetwork)
            {
                return;
            }
            BallListener ball = collision.collider.GetComponent<BallListener>();

            if (ball)
            {
                OnHitBall(ball);
            }
            else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Board"))
            {
                OnHitBoard();
            }
        }

        //public void OnTriggerExit(Collider other)
        //{
            
        //}

        public void OnHitBall(BallListener ball)
        {
            pocketId = -1;
            hitShapeId = ball.id;
            physicsManager.CallBallHitBall(this, ball, true);
        }

        public void OnHitBoard()
        {
            pocketId = -1;
            hitShapeId = -1;
            physicsManager.CallBallHitBoard(this, true);
        }

        void Awake()
        {
            radius = body.GetComponent<SphereCollider>().radius;
            pocketId = -1;
            hitShapeId = -2;
            inMove = physicsManager.inMove;
        }

        void FixedUpdate()
        {
            if (!body.isKinematic && !body.IsSleeping() && physicsManager.inMove)
            {
                physicsManager.CallBallMove(id, body.position, body.linearVelocity, body.angularVelocity);
            }
            if (inMove != physicsManager.inMove)
            {
                inMove = physicsManager.inMove;
                if (!inMove && !body.isKinematic)
                {
                    body.Sleep();
                }
            }
        }
    }
}
