using ColossalFramework.UI;
using UnityEngine;

namespace Freewalking
{
    public class ModeButton : UIButton
    {
        public override void Start()
        {
            base.Start();
            // Set the text to show on the button.
            text = "Enter";

            // Set the button dimensions.
            width = 100;
            height = 30;

            // Style the button to look like a menu 
            normalBgSprite = "ButtonMenu";
            disabledBgSprite = "ButtonMenuDisabled";
            hoveredBgSprite = "ButtonMenuHovered";
            focusedBgSprite = "ButtonMenuFocused";
            pressedBgSprite = "ButtonMenuPressed";
            textColor = new Color32(255, 255, 255, 255);
            disabledTextColor = new Color32(7, 7, 7, 255);
            hoveredTextColor = new Color32(7, 132, 255, 255);
            focusedTextColor = new Color32(255, 255, 255, 255);
            pressedTextColor = new Color32(30, 30, 44, 255);

            // Enable button sounds.
            playAudioEvents = true;

            // Place the button.
            transformPosition = new Vector3(0.5f, 0, 0);

            eventClick += ButtonClick;
        }
        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Player player = new Player();
            player.player.SetActive(true);
        }
    }
}