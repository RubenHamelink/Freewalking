using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Plugins;
using Freewalking.Loaders.Collision;
using Freewalking.UI;
using ICities;
using UnityEngine;

namespace Freewalking.Loaders
{
    public class BuildingCollisionLoader : CollisionLoader
    {
        private BuildingManager buildingManager;

        public override void OnCreated(ILoading loading)
        {
            buildingManager = Singleton<BuildingManager>.instance;
        }

        protected override ushort GetNearestObjectId(ICamera camera, Vector3 position)
        {
            return camera.GetNearestBuilding(position.x, position.y, position.z);
        }

        protected override Mesh GetMesh(ushort id)
        {
            Building building = buildingManager.m_buildings.m_buffer[id];
            return building.Info.m_mesh;
        }

        protected override void CalculateMeshPosition(ushort id, out Vector3 meshPosition, out Quaternion meshRotation)
        {
            Building building = buildingManager.m_buildings.m_buffer[id];
            building.CalculateMeshPosition(out meshPosition, out meshRotation);
        }
    }
}