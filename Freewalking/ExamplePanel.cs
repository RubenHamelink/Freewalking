using ColossalFramework.UI;
using UnityEngine;

namespace TestModCities
{
    public class ExamplePanel : UIPanel
    {
        public override void Start()
        {
            this.backgroundSprite = "GenericPanel";
            this.color = new Color32(0, 255, 0, 100);
            this.width = 100;
            this.height = 200;
        }
    }
}