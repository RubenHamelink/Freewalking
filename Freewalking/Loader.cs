using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Freewalking.UI;
using ICities;
using Microsoft.Win32;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Freewalking
{
    public class Loader : ILoadingExtension
    {
        private List<GameObject> toUnload = new List<GameObject>();

        // Thread: Main
        public void OnCreated(ILoading loading)
        {
//            for (uint i = 0; i < PrefabCollection<UIInfo>.PrefabCount(); ++i)
//            {
//                string name = PrefabCollection<CitizenInfo>.PrefabName(i);
//                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, i + ":" + name);
//            }
        }

        public void OnReleased()
        {
            
        }

        public void OnLevelLoaded(LoadMode mode)
        {
//            List<UIButton> buttons = Object.FindObjectsOfType<UIButton>().ToList();
//            foreach (UIButton button in buttons)
//            {
//                if (!button.tooltip.Contains("Free Camera Mode")) continue;
//                UIPanel infoPanel = (UIPanel) button.parent;
//                FreeWalkButton freeWalkButton =
//                    infoPanel.AddUIComponentIfNotExists<FreeWalkButton>("FreeWalkButton");
//                freeWalkButton.relativePosition = button.relativePosition + Vector3.left * (button.width * 2 + 20);
//                freeWalkButton.SetTemplate(button);
//                freeWalkButton.eventClicked += (component, param) =>
//                {
//                    Player player = new Player();
//                    player.player.SetActive(true);
//                };
//            }

//            UIView v = UIView.GetAView();
//            v.AddUIComponent(typeof(SettingsPanel));
            /*
            GameObject terrainGo = new GameObject();
            Terrain terrain = terrainGo.AddComponent<Terrain>();
            terrain.terrainData = new TerrainData();
            
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Hi there");
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    terrain.terrainData.SetHeights(x, y, new float[,]
                    {
                        {0, 1}
                    });
                }
            }
            
            terrainGo.transform.position = Camera.main.transform.position;*/
//            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
//            plane.transform.localScale = new Vector3(100, 100, 100);
//            plane.transform.position = Camera.main.transform.position;
//            TerrainMesh.GetLodMesh(TerrainMesh.Flags.FullRes, out Mesh mesh, out Quaternion quat);
//            plane.GetComponent<MeshFilter>().mesh = mesh;
//            plane.GetComponent<MeshFilter>().sharedMesh = mesh;
//            toUnload.Add(plane);
//            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, mesh.name);
//            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, mesh.vertexCount.ToString());
        }

        public void OnLevelUnloading()
        {
            toUnload.ForEach(Object.Destroy);
        }
    }
}