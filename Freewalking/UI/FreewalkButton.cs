using ColossalFramework.UI;

namespace Freewalking.UI
{
    public class FreeWalkButton : UIButton
    {
        public FreeWalkButton()
        {
            name = "FreeWalkButton";
            tooltip = "Enter first person mode";
        }

        public void SetTemplate(UIButton button)
        {
            normalFgSprite = button.normalFgSprite;
            normalBgSprite = button.normalBgSprite;
            clickSound = button.clickSound;
            anchor = button.anchor;
            arbitraryPivotOffset = button.arbitraryPivotOffset;
            autoSize = button.autoSize;
            atlas = button.atlas;
            bottomColor = button.bottomColor;
            buttonsMask = button.buttonsMask;
            disabledBgSprite = button.disabledBgSprite;
            disabledFgSprite = button.disabledFgSprite;
            disabledBottomColor = button.disabledBottomColor;
            disabledClickSound = button.disabledClickSound;
            disabledColor = button.disabledColor;
            focusedBgSprite = button.focusedBgSprite;
            focusedFgSprite = button.focusedFgSprite;
            focusedColor = button.focusedColor;
            hoveredBgSprite = button.hoveredBgSprite;
            hoveredFgSprite = button.hoveredFgSprite;
            hoveredColor = button.hoveredColor;
            pressedBgSprite = button.pressedBgSprite;
            pressedFgSprite = button.pressedFgSprite;
            pressedColor = button.pressedColor;
        }
    }
}
