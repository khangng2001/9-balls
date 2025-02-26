using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkManagement
{
    public abstract class ContentListManager : MonoBehaviour
    {
        [SerializeField] protected Button contentButton;
        [SerializeField] protected float buttonsVerticalDistance = 20.0f;
        [SerializeField] protected float buttonsHorizontalDistance = 30.0f;

        private List<Button> buttons;

        public Button GetCurrentButton(int number)
        {
            Button currentButton = Button.Instantiate(contentButton);
            RectTransform buttonRect = contentButton.GetComponent<RectTransform>();
            RectTransform currentRect = currentButton.GetComponent<RectTransform>();

            currentRect.SetParent(contentButton.transform.parent);
            currentRect.localPosition = buttonRect.localPosition - (float)number * (buttonRect.sizeDelta.y + buttonsVerticalDistance) * Vector3.up;
            currentRect.localScale = Vector3.one;

            currentButton.gameObject.SetActive(true);

            return currentButton;
        }

        public void RessetOldButtons(bool hideContentButton = true)
        {
            if (hideContentButton)
            {
                contentButton.gameObject.SetActive(false);
            }
            if (buttons != null)
            {
                foreach (Button playerUI in buttons)
                {
                    Destroy(playerUI.gameObject);
                }
            }
            buttons = new List<Button>(0);
        }

        public void AddButton(Button button)
        {
            buttons.Add(button);
        }
    }
}
