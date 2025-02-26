using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NetworkManagement;

public class FacebookWindowManager : MonoBehaviour 
{
    [SerializeField] private Text publicProfileText;
    [SerializeField] private Text publishActionsText;
    [SerializeField] private Text allActionsText;

	void OnEnable()
    {
        return;
        if (NetworkManager.social.IsLoggedIn() && NetworkManager.social.IsLoggedInWithPublishPermissions())
        {
            publishActionsText.enabled = false;
            publicProfileText.enabled = false;
            allActionsText.enabled = false;
        }
        else if (NetworkManager.social.IsLoggedIn())
        {
            publishActionsText.enabled = true;
            publicProfileText.enabled = false;
            allActionsText.enabled = false;
        }
        else if (NetworkManager.social.IsLoggedInWithPublishPermissions())
        {
            publishActionsText.enabled = false;
            publicProfileText.enabled = true;
            allActionsText.enabled = false;
        }
        else
        {
            publishActionsText.enabled = false;
            publicProfileText.enabled = false;
            allActionsText.enabled = true;
        }
	}
}
