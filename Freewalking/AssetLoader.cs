using System;
using System.Collections.Generic;
using ColossalFramework.Plugins;
using ICities;

namespace Freewalking
{
    public class AssetLoader : IAssetDataExtension
    {
        public IManagers managers { get; }

        public void OnCreated(IAssetData assetData)
        {
        }

        public void OnReleased()
        {
        }

        public void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData)
        {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, name);
        }

        public void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData)
        {
            userData = new Dictionary<string, byte[]>();
        }
    }
}