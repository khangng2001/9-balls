using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using NetworkManagement;
using BallPool;
using BallPool.Mechanics;

/// <summary>
/// Game user interface controller.
/// </summary>
public class GameUIController : MonoBehaviour
{
    /// <summary>
    /// Occurs when on shot.
    /// </summary>
    public event Action<bool> OnShot;
    /// <summary>
    /// Occurs when on force go home.
    /// </summary>
    public event Action OnForceGoHome;
    [SerializeField] private PhysicsManager physicsManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RectTransform replayUI;
    [SerializeField] private RectTransform gameUI;
    [SerializeField] private Text cameraToggleText;
    [SerializeField] private Toggle shotOnUpToggle;
    [SerializeField] private Button shotButton;
    [SerializeField] private Camera tableCamera;
    [SerializeField] private Dropdown replayNumber;
    [SerializeField] private RectTransform camera3DTargetngImage;
    /// <summary>
    /// Gets the replay number value.
    /// </summary>
    /// <value>The replay number value.</value>
    public int replayNumberValue
    {
        get;
        private set;
    }

    /// <summary>
    /// The camera2 d.
    /// </summary>
    public Camera camera2D;
    [SerializeField] private Camera cueCamera;
    /// <summary>
    /// The is3 d.
    /// </summary>
    [System.NonSerialized] public bool is3D;
    /// <summary>
    /// The shot on up.
    /// </summary>
    [System.NonSerialized] public bool shotOnUp;
    [SerializeField] private Slider forceSlider;
    [SerializeField] private string homeScene;
    [SerializeField] private string playScene;


    void Awake ()
    {
        if (!NetworkManager.initialized)
        {
            enabled = false;
            return;
        }

        camera2D.orthographicSize *= 1.6f / ((float)Screen.width / (float)Screen.height);
        if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
        {
            is3D = false;
        }
        else
        {
            is3D = DataManager.GetIntData("Is3D") == 1;
        }
        camera3DTargetngImage.gameObject.SetActive(is3D && gameManager.shotController.cueControlType == ShotController.CueControlType.ThirdPerson);

        shotOnUp = DataManager.GetIntData("ShotOnUp") == 0;
        shotButton.image.enabled = !shotOnUp;
        shotButton.enabled = !shotOnUp;
        shotButton.GetComponentInChildren<Text>().text = shotOnUp ? "Auto shot" : "";
        shotOnUpToggle.isOn = shotOnUp;
        CameraToggle();
        ControlChanged();
        OnTriggerAutoShot();
    }
    void Start()
    {
        if (BallPoolGameLogic.playMode == BallPool.PlayMode.Replay)
        {
            replayNumber.options = new List<Dropdown.OptionData>(0);
            replayNumberValue = 0;
            int replayCount = physicsManager.replayManager.GetReplayDataCount();
            Debug.Log("replayCount " + replayCount);
            for (int i = 0; i < replayCount; i++)
            {
                replayNumber.options.Add(new Dropdown.OptionData("Replay " + i));
            }
            replayNumber.value = 1;
            replayNumber.value = 0;
        }
    }
    /// <summary>
    /// Raises the replay number changed event.
    /// </summary>
    public void OnReplayNumberChanged()
    {
        if (BallPoolGameLogic.playMode == BallPool.PlayMode.Replay)
        {
            replayNumberValue = replayNumber.value;
            Debug.Log("replayNumber " + replayNumberValue);
            gameManager.SetBallsState(replayNumber.value);
        }
    }
        
    /// <summary>
    /// Sets the replay number.
    /// </summary>
    public void SetReplayNumber(int replayNumberValue, bool setReplay)
    {
        this.replayNumberValue = replayNumberValue;
        replayNumber.value = replayNumberValue;
        if (setReplay)
        {
            gameManager.SetBallsState(replayNumber.value);
        }
    }
    /// <summary>
    /// Raises the control changed event.
    /// </summary>
    public void OnControlChanged()
    {
        ControlChanged();
    }
    /// <summary>
    /// Controls the changed.
    /// </summary>
    public void ControlChanged()
    {
        replayUI.gameObject.SetActive(BallPoolGameLogic.playMode == BallPool.PlayMode.Replay);
        gameUI.gameObject.SetActive(!replayUI.gameObject.activeSelf);
        OnReplayNumberChanged();
    }

    /// <summary>
    /// Raises the camera toggle event.
    /// </summary>
    public void OnCameraToggle()
    {
        is3D = !is3D;
        camera3DTargetngImage.gameObject.SetActive(is3D && gameManager.shotController.cueControlType == ShotController.CueControlType.ThirdPerson);
        DataManager.SetIntData("Is3D", is3D ? 1 : 0);
        CameraToggle();
    }

    /// <summary>
    /// Raises the trigger auto shot event.
    /// </summary>
    public void OnTriggerAutoShot()
    {
        shotOnUp = shotOnUpToggle.isOn;
        shotButton.GetComponent<Image>().enabled = !shotOnUp;
        shotButton.image.enabled = !shotOnUp;
        shotButton.enabled = !shotOnUp;
        shotButton.GetComponentInChildren<Text>().text = shotOnUp ? "Auto shot" : "";
        DataManager.SetIntData("ShotOnUp", shotOnUp ? 0 : 1);
        forceSlider.gameObject.SetActive(!(shotOnUp && !InputOutput.isMobilePlatform));
    }
 
    public void Shot(bool follow)
    {
        if (OnShot != null)
        {
            OnShot(follow);
        }
    }

    /// <summary>
    /// Play this instance.
    /// </summary>
    public void Play()
    {
        physicsManager.Disable();
        SceneManager.LoadScene(playScene);
    }

    /// <summary>
    /// Gos the home.
    /// </summary>
    public void GoHome()
    {
        physicsManager.Disable();
        if (NetworkManager.mainPlayer != null)
        {
            NetworkManager.mainPlayer.state = PlayerState.Online;
        }
        SceneManager.LoadScene(homeScene);
    }
    /// <summary>
    /// Forces the go home.
    /// </summary>
    public void ForceGoHome()
    {
        Debug.LogWarning("opponenIsReadToPlay " +  gameManager.shotController.opponenIsReadToPlay);
        if(gameManager.shotController.opponenIsReadToPlay)
        {
            if (BallPoolGameLogic.playMode == BallPool.PlayMode.OnLine)
            {
                if (OnForceGoHome != null)
                {
                    OnForceGoHome();
                }
            }
        }
        else
        {
            BallPoolPlayer.ReturnMainPlayerCouns();
            BallPoolPlayer.SaveCoins();
        }

        GoHome();
    }
    /// <summary>
    /// Cameras the toggle.
    /// </summary>
    public void CameraToggle()
    {
        camera2D.enabled = !is3D;
        tableCamera.enabled = is3D;
        cameraToggleText.text = is3D ? "2D" : "3D";
        InputOutput.usedCamera = is3D ? cueCamera : camera2D;
    }
}
