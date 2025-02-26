using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallPool.Mechanics;

namespace BallPool
{
    public class AightBallPoolBallsSortingManager : MonoBehaviour, BallPoolBallsSortingManager
    {
        [SerializeField] private Transform balls;
        [SerializeField] private Transform BallsListener;
        [SerializeField] private PhysicsManager physicsManager;
        [SerializeField] private float ballsDistance;
        [SerializeField] Transform cueBallPosition;
        [SerializeField] Transform pyramidFirstBallPosition;
        [SerializeField] private GameManager gameManager;

        public void SortEightBalls()
        {
            Debug.Log("Balls sorted with eight-balls rule");
            Vector2[] delta = { 
                new Vector2(0.0f, 0.0f),//0
                new Vector2(4.0f, 4.0f),//15
                new Vector2(1.0f, -1.0f),//2
                new Vector2(2.0f, 2.0f),//9
                new Vector2(3.0f, -3.0f),//10
                new Vector2(3.0f, 1.0f),//8
                new Vector2(4.0f, -4.0f),//3
                new Vector2(4.0f, 0.0f),//4
                new Vector2(2.0f, 0.0f),//11
                new Vector2(1.0f, 1.0f),//5
                new Vector2(2.0f, -2.0f),//12
                new Vector2(3.0f, -1.0f),//6
                new Vector2(3.0f, 3.0f),//13
                new Vector2(4.0f, -2.0f),//7
                new Vector2(4.0f, 2.0f),//14
                new Vector2(0.0f, 0.0f)//1
                
            };

            gameManager.balls = new Ball[balls.childCount];
            physicsManager.ballsListener = new BallListener[balls.childCount];
            
            for (int i = 0; i < balls.childCount; i++)
            {
                Ball ball = balls.GetChild(i).GetComponent<Ball>();
                BallListener listener = BallsListener.GetChild(i).GetComponent<BallListener>();
                
                ball.gameObject.SetActive(true);
                listener.gameObject.SetActive(true);
                
                listener.body = listener.GetComponent<Rigidbody>();
                ball.listener = listener;
                float distance = listener.GetComponent<SphereCollider>().radius + ballsDistance;
                
                Vector3 position = cueBallPosition.position;
                if (i != 0)
                {
                    position = pyramidFirstBallPosition.position + new Vector3(delta[i].x * Mathf.Sqrt(Mathf.Pow(2.0f * distance, 2.0f) - Mathf.Pow(distance, 2.0f)), 0.0f, delta[i].y * distance);
                }
                ball.id = i;
                ball.transform.position = position;
                listener.transform.position = position;
                listener.id = ball.id;
                listener.physicsManager = physicsManager;
                ball.name = listener.name = "Ball_" + i;
                
                gameManager.balls[i] = ball;
                physicsManager.ballsListener[i] = listener;
            }
            
        }

        public void SortNineBalls()
        {
            Debug.Log("Balls sorted with 9-balls rule");
            Vector2[] delta = 
            {
                new Vector2(0.0f, 0.0f),    // 0 (Cue ball)
                new Vector2(0.0f, 0.0f),    // 1 ball (front)
                new Vector2(1.0f, -1.0f),    // 2 ball
                new Vector2(1.0f, 1.0f),   // 3 ball
                
                new Vector2(2.0f, -2.0f),   // 4 ball
                new Vector2 (2.0f, 2.0f),   // 5 ball
                
                new Vector2(3.0f, -1.0f),    // 6 ball
                new Vector2(3.0f, 1.0f),   // 7 ball
                
                new Vector2(4.0f, 0.0f),    // 8 ball
                new Vector2(2.0f, 0.0f),   // 9 ball (center)
                Vector2.zero,               // Unused balls (10-15)
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero
            };
            gameManager.balls = new Ball[balls.childCount];
            physicsManager.ballsListener = new BallListener[balls.childCount];

            for (int i = 0; i < balls.childCount; i++)
            {
                Ball ball = balls.GetChild(i).GetComponent<Ball>();
                BallListener listener = BallsListener.GetChild(i).GetComponent<BallListener>();
                listener.body = listener.GetComponent<Rigidbody>();
                ball.listener = listener;
                float distance = listener.GetComponent<SphereCollider>().radius + ballsDistance;
               
                Vector3 position = cueBallPosition.position;
                if (i != 0 && i < 10)
                {
                    position = pyramidFirstBallPosition.position + new Vector3(delta[i].x * Mathf.Sqrt(Mathf.Pow(2.0f * distance, 2.0f) - Mathf.Pow(distance, 2.0f)), 0.0f, delta[i].y * distance);
                }
                else if (i >= 10)
                {
                    balls.GetChild(i).gameObject.SetActive(false);
                    listener.gameObject.SetActive(false);
                }
                
                ball.id = i;
                ball.transform.position = position;
                listener.transform.position = position;
                listener.id = ball.id;
                listener.physicsManager = physicsManager;
                ball.name = listener.name = "Ball_" + i;

                gameManager.balls[i] = ball;
                physicsManager.ballsListener[i] = listener;
            }
            ReCalculateBallsList();
        }

        private void ReCalculateBallsList()
        {
            Ball [] newListBall = new Ball[10];
            BallListener [] newListListener = new BallListener[10];

            for (int i = 0; i < 10; i++)
            {
                newListBall[i] = gameManager.balls[i];
                newListListener[i] = physicsManager.ballsListener[i];
            }
            gameManager.balls = newListBall;
            physicsManager.ballsListener = newListListener;
        }
    }
}
