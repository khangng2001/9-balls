using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;
using BallPool.Mechanics;

namespace BallPool
{
    /// <summary>
    /// 3D and 2D mode manager.
    /// </summary>
    public class ThreDTwoDManager : MonoBehaviour
    {
        [SerializeField] private ShotController shotController;
        [SerializeField] private RectTransform cueBallTargetngImage;

        [SerializeField] private GameObject threDTable;
        [SerializeField] private GameObject twoDTable;
        [SerializeField] private Transform ballsConteyner;
        private Transform[] balls;
        private Transform[] ballsShadow;
        private Transform[] ballsBlick;
        [SerializeField] private Transform ballShadow;
        [SerializeField] private Transform ballBlick;
        [SerializeField] private Material balls3DMaterial;
        [SerializeField] private Material balls2DMaterial;

        [SerializeField] private MeshRenderer ballChecker;
        [SerializeField] private Material ballChecker3DMaterial;
        [SerializeField] private Material ballChecker2DMaterial;

        [SerializeField] private MeshRenderer hand;
        [SerializeField] private Material[] hand3DMaterial;
        [SerializeField] private Material[] hand2DMaterial;

        [SerializeField] private GameObject cue3D;
        [SerializeField] private GameObject cue2D;
        [SerializeField] private RectTransform[] hideOn2D;
        [SerializeField] private RectTransform[] hideOn3D;


        void Awake()
        {
            if (!NetworkManager.initialized)
            {
                return;
            }
            threDTable.SetActive(AightBallPoolNetworkGameAdapter.is3DGraphics);
            twoDTable.SetActive(!AightBallPoolNetworkGameAdapter.is3DGraphics);
            balls = new Transform[ballsConteyner.childCount];
            ballsShadow = new Transform[balls.Length];
            ballsBlick = new Transform[balls.Length];
            ballChecker.sharedMaterial = AightBallPoolNetworkGameAdapter.is3DGraphics ? ballChecker3DMaterial : ballChecker2DMaterial;
            hand.sharedMaterials = AightBallPoolNetworkGameAdapter.is3DGraphics ? hand3DMaterial : hand2DMaterial;
            cue3D.SetActive(AightBallPoolNetworkGameAdapter.is3DGraphics);
            cue2D.SetActive(!AightBallPoolNetworkGameAdapter.is3DGraphics);

            for (int i = 0; i < ballsConteyner.childCount; i++)
            {
                balls[i] = ballsConteyner.GetChild(i);
                balls[i].GetComponent<MeshRenderer>().sharedMaterial = AightBallPoolNetworkGameAdapter.is3DGraphics ? balls3DMaterial : balls2DMaterial;

                ballsShadow[i] = Transform.Instantiate(ballShadow) as Transform;
                ballsShadow[i].transform.parent = transform;
                ballsShadow[i].transform.position = balls[i].position;

                Ball ball = balls[i].GetComponent<Ball>();
                ball.lightCentre = this.transform;
                ball.ballShadow = ballsShadow[i];

                if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
                {
                    ballsBlick[i] = Transform.Instantiate(ballBlick) as Transform;
                    ballsBlick[i].transform.parent = transform;
                    ballsBlick[i].transform.position = balls[i].position;

                    ball.ballBlick = ballsBlick[i];
                }
            }
            foreach (var item in hideOn3D)
            {
                item.gameObject.SetActive(!AightBallPoolNetworkGameAdapter.is3DGraphics);
            }
            foreach (var item in hideOn2D)
            {
                item.gameObject.SetActive(AightBallPoolNetworkGameAdapter.is3DGraphics);
            }

            if (shotController.cueControlType == ShotController.CueControlType.ThirdPerson)
            {
                cueBallTargetngImage.gameObject.SetActive(true);
            }

            Destroy(ballShadow.gameObject);
            Destroy(ballBlick.gameObject);
            enabled = false;
        }
    }
}
