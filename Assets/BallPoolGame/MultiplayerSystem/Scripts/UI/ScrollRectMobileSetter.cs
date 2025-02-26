using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkManagement
{
    public class ScrollRectMobileSetter : MonoBehaviour
    {
        void Start()
        {
            if (Application.isMobilePlatform)
            {
                GetComponent<ScrollRect>().inertia = false;
            }
        }
    }
}
