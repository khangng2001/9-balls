using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkManagement
{
    /// <summary>
    /// Product user interface tool, it is a tool for managing products properties.
    /// </summary>
    public abstract class ProductUITool : MonoBehaviour
    {
        public string productType;
        public RectTransform panel;
    }
}
