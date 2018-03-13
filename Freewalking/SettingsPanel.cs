using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace Freewalking
{
    public class SettingsPanel : UIPanel
    {
        private UIPanel panel;

        public static float speedMod = 1;

        public override void Start()
        {
            float deltaHorz = Screen.width / 1680f;
            float deltaVert = Screen.height / 1050f;
            UIPanel button = this.AddUIComponent<UIPanel>();
            UIHelper uip = new UIHelper(button);
            uip.AddButton("Freewalking", OpenSettings);
            button.transformPosition = new Vector3(1.2f * deltaHorz, 0.975f * deltaVert);
            button.width = 120 * deltaHorz;
            button.height = 30 * deltaVert;

            panel = this.AddUIComponent<UIPanel>();
            
            UIHelper ui = new UIHelper(panel);
            UIHelperBase settingsGroup = ui.AddGroup("This screen is dragable!");
            ((settingsGroup as UIHelper).self as UIComponent).width = 300 * deltaHorz;
            UIHelperBase settings = settingsGroup.AddGroup("Freewalking Settings");
            ((settings as UIHelper).self as UIComponent).width = 300 * deltaHorz;
            settings.AddButton("Enter FPM", OnEnterFPM);
            settings.AddSlider("Sensitivity", 1, 50, 1, 10, OnSensChange);
            settings.AddSlider("Speed", 0.5f, 1.5f, 0.1f, 1f, OnSpeedChange);

            UIHelperBase controls = settingsGroup.AddGroup("Controls");
            ((controls as UIHelper).self as UIComponent).width = 300 * deltaHorz;
            controls.AddTextfield("- Escape -", "Exit FPM", empty);
            controls.AddTextfield("- Control -", "Free Cursor", empty);
            controls.AddTextfield("- E -", "Enter and exit vehicle", empty);
            controls.AddTextfield("- Space -", "Jump", empty);
            controls.AddTextfield("- Mouse Wheel -", "Third person mode", empty);

            UIHelperBase news = settingsGroup.AddGroup("What's new");
            ((news as UIHelper).self as UIComponent).width = 300 * deltaHorz;
            news.AddTextfield("- Jumping!", "- Third person!", empty);
            news.AddTextfield("- Improved core functionality!", "- Physics!", empty);

            panel.backgroundSprite = "GenericPanel";
            panel.width = 300 * deltaHorz;
            panel.height = 850 * deltaVert;
            panel.transformPosition = new Vector3(1.05f * deltaHorz, 0.85f * deltaVert);
            this.width = 0;
            panel.isVisible = false;
        }

        private void empty(string text) { }

        private void OpenSettings()
        {
            panel.isVisible = !panel.isVisible;
        }

        private void OnEnterFPM()
        {
            Player player = new Player();
            player.player.SetActive(true);
            panel.isVisible = false;
        }

        private void OnSensChange(float sens)
        {
            PlayerControl.sensitivity = sens;
        }

        private void OnSpeedChange(float speed)
        {
            speedMod = speed;
        }

        bool dragging = false;
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            dragging = true;
            base.OnMouseDown(p);
        }
        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            dragging = false;
            base.OnMouseUp(p);
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {

            if (dragging) this.position = new Vector3(this.position.x + p.moveDelta.x,
               this.position.y + p.moveDelta.y,
               this.position.z);
            base.OnMouseMove(p);
        }

    }
}