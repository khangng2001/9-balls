using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BallPool;

/// <summary>
/// Manager for targeting on cue ball in 2D mode.
/// </summary>
public class Targeting2DManager : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private RectTransform point;
    [SerializeField] private ShotController shotController;
    private RectTransform rectTransform;
    private float radius;
    private float currentRadius;
    private Vector3 localPosition;
    private Vector3 checkLocalPosition;
    private bool canControl = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    void Start()
    {
        radius = 0.5f * (rectTransform.sizeDelta.x - point.sizeDelta.x);
    }
    void OnEnable()
    {
        localPosition = point.localPosition;
        Debug.Log("OnEnable");
        InputOutput.OnMouseState += InputOutput_OnMouseState;
    }
    void OnDisable()
    {
        InputOutput.OnMouseState -= InputOutput_OnMouseState;
    }
    void InputOutput_OnMouseState (MouseState mouseState)
    {
        if (canControl && mouseState == MouseState.Up)
        {
            ShotController.canControl = true;
            canControl = false;
            shotController.RessetCueAfterTargeting();
        }
        if (canControl && mouseState == MouseState.PressAndMove)
        {
            localPosition -= 0.3f * InputOutput.mouseScreenSpeed * Time.deltaTime;
            currentRadius = Mathf.Sqrt(localPosition.x * localPosition.x + localPosition.y * localPosition.y);
            if (currentRadius < radius)
            {
                checkLocalPosition = localPosition;
                point.localPosition = localPosition;
            }
            else
            {
                localPosition = checkLocalPosition;
            }
            SetCuePosition(-localPosition / radius);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        canControl = true;
        ShotController.canControl = false;
        shotController.RessetCueForTargeting();
    }
    private void SetCuePosition(Vector3 normalizedPosition)
    {
        shotController.SetCueTargetingPosition(normalizedPosition);
    }
    public void Resset()
    {
        point.localPosition = localPosition = Vector3.zero;
        ShotController.canControl = true;
    }
    public void SetPointTargetingPosition(Vector3 normalizedPosition)
    {
        point.localPosition = normalizedPosition * radius;
    }
}
