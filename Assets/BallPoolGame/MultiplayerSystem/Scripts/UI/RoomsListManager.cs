using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkManagement
{
    public delegate void SelecRoomHandler(NetworkManagement.Room room);
    public delegate void SelecPlayerHandler(NetworkManagement.PlayerProfile player);

    /// <summary>
    /// The rooms list manager.
    /// </summary>
    public class RoomsListManager : ContentListManager 
    {
        public static event SelecPlayerHandler OnSelecPlayerProfile;
        public static event SelecRoomHandler OnSelecRoom;
    	
        public void UpdateRooms(NetworkManagement.Room[] rooms)
        {
            RessetOldButtons();

            if (rooms == null || rooms.Length == 0)
            {
                return;
            }
            for (int i = 0; i < rooms.Length; i++)
            {
                Button currentButton = GetCurrentButton(i);

                PlayerProfileUI playerUI = currentButton.GetComponentInChildren<PlayerProfileUI>();
                RectTransform playerRect = playerUI.GetComponent<RectTransform>();
                playerUI.gameObject.SetActive(false);

                NetworkManagement.Room currentRoom = rooms[i];

                for (int j = 0; j < rooms[i].players.Count; j++) 
                {
                    PlayerProfileUI currentPlayerUI = PlayerProfileUI.Instantiate(playerUI);
                    RectTransform currentPlayerRect = currentPlayerUI.GetComponent<RectTransform>();
                    currentPlayerRect.SetParent(playerUI.transform.parent);

                    currentPlayerRect.localPosition = playerRect.localPosition + (float)j * (playerRect.sizeDelta.x + buttonsHorizontalDistance) * Vector3.right;
                    currentPlayerRect.localScale = Vector3.one;

                    NetworkManagement.PlayerProfile currentPlayerProfile = currentRoom.players[j];
                    currentPlayerUI.SetPlayer(currentPlayerProfile);

                    currentPlayerUI.gameObject.SetActive(true);
                }

                NetworkManagement.Pointer currentPointer = currentButton.GetComponent<Pointer>();

                if (currentPointer && Application.isMobilePlatform)
                {

                    currentPointer.OnSelecButton += (Pointer pointer) =>
                    {
                            if (OnSelecRoom != null)
                            {
                                OnSelecRoom(currentRoom);
                            }
                    };
                }
                else
                {
                    currentButton.onClick.AddListener(() =>
                        {
                            if (OnSelecRoom != null)
                            {
                                OnSelecRoom(currentRoom);
                            }
                        });
                }
                AddButton(currentButton);
            }

        }
        public void UpdatePlayers(NetworkManagement.PlayerProfile[] players)
        {
            RessetOldButtons();

            if (players == null || players.Length == 0)
            {
                return;
            }
            for (int i = 0; i < players.Length; i++)
            {
                Button currentButton = GetCurrentButton(i);

                PlayerProfileUI currentProfileUI = currentButton.GetComponent<PlayerProfileUI>();
                NetworkManagement.PlayerProfile currentPlayer = players[i];

                currentProfileUI.SetPlayer(currentPlayer);

                currentButton.onClick.AddListener(() => 
                    {
                        if (OnSelecPlayerProfile != null)
                        {
                            OnSelecPlayerProfile(currentPlayer);
                        }
                    });
                AddButton(currentButton);
            }
        }


    }
}
