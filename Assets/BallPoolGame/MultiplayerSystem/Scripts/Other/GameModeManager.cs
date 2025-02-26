using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
	[SerializeField] private string homeScene;

	public void SetGameMode(int modeId)
	{
		AightBallPoolNetworkGameAdapter.is3DGraphics = modeId == 0;
		SceneManager.LoadScene (homeScene);
	}
}
