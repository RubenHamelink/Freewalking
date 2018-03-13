using ICities;

namespace Freewalking
{

    public class TerrainExtend : ITerrainExtension
    {
        public static ITerrain terrainManager;

        //Thread: Main
        public void OnCreated(ITerrain terrain)
        {
            terrainManager = terrain;
        }
        //Thread: Main
        public void OnReleased()
        {

        }
        //Thread: Simulation
        public void OnAfterHeightsModified(float minX, float minZ, float maxX, float maxZ)
        {


        }

    }
}
