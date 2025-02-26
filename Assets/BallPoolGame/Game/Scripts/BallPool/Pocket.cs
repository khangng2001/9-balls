using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallPool.Mechanics;
using Mechanics;

namespace BallPool
{
    public class Pocket : MonoBehaviour
    {
        public class BallRoll
        {
            public BallListener ball{ get; private set; }
            public float maxLength{ get; private set; }

            public BallRoll(BallListener ball, float maxLength)
            {
                this.ball = ball;
                this.maxLength = maxLength;
            }
        }
        private PhysicsManager physicsManager;
        public int id;
        private Vector3[] nodes;
        private float length;
        private float currentLength;

        void Start()
        {
            physicsManager = PhysicsManager.FindObjectOfType<PhysicsManager>();
            if (!physicsManager)
            {
                Destroy(gameObject);
                return;
            }
            physicsManager.OnBallHitPocket += PhysicsManager_OnBallHitPocket;
            physicsManager.OnBallExitFromPocket += PhysicsManager_OnBallExitFromPocket;
            nodes = new Vector3[transform.childCount];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = transform.GetChild(i).position;
            }
            length = QuadraticCurve.CalculateLength(nodes); 
            currentLength = length;
        }

        void OnDisable()
        {
            if (physicsManager)
            {
                physicsManager.OnBallHitPocket -= PhysicsManager_OnBallHitPocket;
                physicsManager.OnBallExitFromPocket -= PhysicsManager_OnBallExitFromPocket;
            }
        }
        void OnDestroy()
        {
            if (physicsManager)
            {
                physicsManager.OnBallHitPocket -= PhysicsManager_OnBallHitPocket;
                physicsManager.OnBallExitFromPocket -= PhysicsManager_OnBallExitFromPocket;
            }
        }
        void PhysicsManager_OnBallExitFromPocket (BallListener ball, PocketListener pocket, BallExitType exitType, bool inMove)
        {
            if(pocket.id == id)
            {
                currentLength += 2.0f * ball.radius;
            }
        }

        void PhysicsManager_OnBallHitPocket (BallListener ball, PocketListener pocket, bool inMove)
        {
            if(pocket.id == id)
            {
                ball.pocket = pocket;
                OnBallHit(ball, inMove);
            }
        }

        void OnBallHit(BallListener ball, bool inMove)
        {
            BallRoll roll = new BallRoll(ball, currentLength);

            ball.body.isKinematic = true;
            GameManager.Instance.DeactiveBall(ball.id);
            BallsUIManager.Instance.NullOneBall(ball.id);
            StartCoroutine(RollTheBall(roll));
            currentLength -= 2.0f * ball.radius;
        }
//        void Update()
//        {
//            QuadraticCurve.CalculateLength(10, nodes); 
//        }
        IEnumerator RollTheBall(BallRoll roll)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForEndOfFrame();
            float time = 0.0f;
            Vector3 ballOldPosition = roll.ball.body.position;
            while (time < roll.maxLength && roll.ball.pocket && roll.ball.body.isKinematic) 
            {
                roll.ball.transform.position = QuadraticCurve.CalculateValue(time / length, nodes);
                Vector3 ballVelocity = (roll.ball.body.position - ballOldPosition) / Time.fixedDeltaTime;
                ballOldPosition = roll.ball.body.position;
                roll.ball.transform.Rotate(Vector3.Cross(ballVelocity, Vector3.up) / roll.ball.radius);
                time += 0.5f * Time.fixedDeltaTime;
                physicsManager.CallBallMove(roll.ball.id, roll.ball.body.position, Vector3.zero, Vector3.zero);
                yield return new WaitForFixedUpdate();
            }
            if (roll.ball.pocket && roll.ball.body.isKinematic)
            {
                roll.ball.transform.position = QuadraticCurve.CalculateValue(0.99f * roll.maxLength / length, nodes);
                physicsManager.CallBallMove(roll.ball.id, roll.ball.body.position, Vector3.zero, Vector3.zero);
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
