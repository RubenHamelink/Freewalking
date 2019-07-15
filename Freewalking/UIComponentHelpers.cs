using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Freewalking.UI;

namespace Freewalking
{
    public static class UIComponentHelpers
    {
        public static T AddUIComponentIfNotExists<T>(this UIComponent parent, string componentName) where T : UIComponent
        {
            T addedComponent = parent.GetComponentsInChildren<T>().FirstOrDefault(b => b.name.Equals(componentName));

            if (addedComponent == null)
            {
                addedComponent = parent.AddUIComponent<T>();
                addedComponent.name = componentName;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, addedComponent.ToString());

            return addedComponent;
        }
    }
}
