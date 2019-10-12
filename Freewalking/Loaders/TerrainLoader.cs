using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Plugins;
using Freewalking.UI;
using ICities;
using UnityEngine;

namespace Freewalking.Loaders
{
    public class TerrainLoader : MonoBehaviour, ITerrainExtension
    {
        private ITerrain terrain;

        private readonly Dictionary<int, Dictionary<int, GameObject>> cells =
            new Dictionary<int, Dictionary<int, GameObject>>();

        private const int CellSize = 50;
        
        public void Update()
        {
            if (!FreewalkingCamera.IsFreewalking)
                return;

            Vector3 cellPosition = GetNearestCell(Camera.main.transform.position);

            CreateCellClusterAt(cellPosition, 1);
        }

        private Vector3 GetNearestCell(Vector3 position)
        {
            return new Vector3(
                (int) (position.x / CellSize),
                0,
                (int) (position.z / CellSize)
            );
        }

        private GameObject GeneratePlane(Vector3 position, int size)
        {
            GameObject plane = new GameObject("plane");
            MeshFilter filter = plane.AddComponent<MeshFilter>();
            MeshCollider collider = plane.AddComponent<MeshCollider>();
            filter.mesh = GenerateMesh(position, size);
            collider.sharedMesh = filter.mesh;
            plane.transform.position = position;
            return plane;
        }

        private Mesh GenerateMesh(Vector3 position, int size)
        {
            Vector3[,] heightMap = GenerateHeightMap(position, size);

            Vector3[] verts = new Vector3[size * size];
            Vector3[] normals = new Vector3[size * size];
            Vector2[] uv = new Vector2[size * size];

            List<int> triangles = new List<int>();

            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    int index = x % size + z * size;
                    verts[index] = heightMap[x, z];
                    normals[index] = Vector3.up;

                    if (x > size - 2 || z > size - 2)
                        continue;

                    // triangle 1
                    triangles.Add(index);
                    triangles.Add(index + size);
                    triangles.Add(index + size + 1);

                    // triangle 2
                    triangles.Add(index);
                    triangles.Add(index + size + 1);
                    triangles.Add(index + 1);
                }
            }

            Mesh mesh = new Mesh
            {
                name = "planeMesh",
                vertices = verts,
                triangles = triangles.ToArray(),
                uv = uv,
                normals = normals
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        private Vector3[,] GenerateHeightMap(Vector3 position, int size)
        {
            Vector3[,] heightMap = new Vector3[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    heightMap[x, z] = new Vector3(x, GetHeight(position.x + x, position.z + z), z);
                }
            }

            return heightMap;
        }

        private float GetHeight(float x, float y)
        {
            return terrain.SampleTerrainHeight(x, y);
        }

        public void OnCreated(ITerrain terrain)
        {
            this.terrain = terrain;
            GameObject terrainLoader = new GameObject("TerrainLoader");
            terrainLoader.AddComponent<TerrainLoader>().terrain = terrain;
        }

        public void CreateCellClusterAt(Vector3 position, int radius)
        {
            for (float x = position.x - radius; x < position.x + radius; x++)
            {
                for (float z = position.z - radius; z < position.z + radius; z++)
                {
                    CreateCellAt(new Vector3(x,0, z));
                }
            }
        }

        public void CreateCellAt(Vector3 position)
        {
            if (CellExistsAt(position))
            {
                return;
            }

            if (!cells.ContainsKey((int) position.x))
                cells.Add((int) position.x, new Dictionary<int, GameObject>());

            cells[(int) position.x].Add((int) position.z,
                GeneratePlane(new Vector3(position.x * CellSize, 0, position.z * CellSize), CellSize + 1));
        }

        public bool CellExistsAt(Vector3 position)
        {
            return cells.ContainsKey((int) position.x) && cells[(int) position.x].ContainsKey((int) position.z);
        }

        public void OnReleased()
        {
        }

        public void OnAfterHeightsModified(float minX, float minZ, float maxX, float maxZ)
        {
        }
    }
}