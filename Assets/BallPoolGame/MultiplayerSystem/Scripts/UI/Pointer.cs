using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void SelecButtonHandler(NetworkManagement.Pointer pointer);

namespace NetworkManagement
{
    public class Pointer : MonoBehaviour, IPointerDownHandler//, IMoveHandler, IPointerUpHandler,
    { 
        public event SelecButtonHandler OnSelecButton;
        private float downTime;
        private bool down;

        void Awake()
        {
            if (!Application.isMobilePlatform)
            {
                Destroy(this);
                return;
            }
            down = false;
            downTime = 0.0f;
        }
        void Update()
        {
            if (down && Input.GetMouseButton(0))
            {
                downTime += Time.deltaTime;
                if (downTime > 0.2f)
                {
                    down = false;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (down)
                {
                    if (OnSelecButton != null)
                    {
                        OnSelecButton(this);
                    }
                    down = false;
                }
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            down = true;
            downTime = 0.0f;
        }
//        public void OnPointerUp(PointerEventData eventData)
//        {
//            if (downTime > 0.2f)
//            {
//                down = false;
//            }
//        }
//        public void OnMove(AxisEventData eventData)
//        {
//            Debug.Log(eventData.moveVector.magnitude);
//            if (downTime > 0.2f)
//            {
//                down = false;
//            }
//        }
    }
}
