using ColossalFramework;
using ICities;
using UnityEngine;

namespace Freewalking
{
    public class PlayerControl : MonoBehaviour
    {
        Camera cam;
        Vector3 cameraOffset;
        Camera origCam;
        CameraController cc;
        MouseLook cml;
        MouseLook gml;
        public static float sensitivity = 10;
        public float cameraZoom = 0;

        public ushort vehicleID = 0;
        ushort oldVehicleID = 0;
        public GameObject vehicleGO;
        public ushort parkedID = 0;
        ushort oldParkedID = 0;
        public GameObject parkedGO;
        bool driving;
        bool following;

        GameObject roadGO;
        public ushort roadID = 0;

        ushort oldBuildingID = 0;
        BuildingManager bm;
        GameObject buildingGO;
        GameObject terrainGO;

        SimulationManager sm;

        void Start()
        {
            InitCams();

            SetCameraOffset("Walking");

            Vector3 pos = origCam.transform.position;
            pos.y = TerrainExtend.terrainManager.SampleTerrainHeight(pos.x, pos.z) + 2;
            gameObject.transform.position = pos;
            Quaternion rot = origCam.transform.rotation;
            rot.x = 0;
            rot.z = 0;
            gameObject.transform.rotation = rot;

            roadGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            roadGO.name = "Road";
            roadGO.GetComponent<MeshRenderer>().enabled = false;
            roadGO.GetComponent<MeshCollider>().convex = false;
            roadGO.layer = 2;

            vehicleGO = new GameObject("Vehicle");
            vehicleGO.AddComponent<BoxCollider>();
            vehicleGO.AddComponent<Rigidbody>().useGravity = false;

            parkedGO = new GameObject("Parked vehicle");
            parkedGO.AddComponent<MeshCollider>();

            buildingGO = new GameObject("Building");
            buildingGO.AddComponent<MeshCollider>();
            bm = Singleton<BuildingManager>.instance;

            terrainGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrainGO.name = "Terrain";
            terrainGO.GetComponent<MeshRenderer>().enabled = false;
            terrainGO.GetComponent<MeshCollider>().convex = false;
            terrainGO.layer = 2;

            sm = FindObjectOfType<SimulationManager>();

            cameraZoom = -3;
        }
        void Update()
        {
            sm.SimulationPaused = false;
            sm.SelectedSimulationSpeed = 1;

            cml.sensitivityY = sensitivity;
            gml.sensitivityX = sensitivity;

            cameraZoom = Mathf.Clamp(cameraZoom + Input.mouseScrollDelta.y, -5, 0);
            cam.transform.localPosition = cameraOffset + new Vector3(0, -cameraZoom / 4, cameraZoom * 2);

            if (driving)
                transform.position = GetComponent<VehicleControl>().vehicleGO.transform.position;

            Vector3 camOffset = transform.forward * 2;
            for(int i = 2; i < 100; i+=20)
            {
                if(bm.FindBuilding(cam.transform.position - transform.forward * i, 20, ItemClass.Service.None, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None) == 0)
                {
                    camOffset = transform.forward * i;
                    break;
                }
            }
            if (cc.m_currentPosition.y - cam.transform.position.y > 30)
            {
                cc.m_targetAngle = new Vector2(transform.rotation.eulerAngles.y, 45f);
            }
            else
            {
                cc.m_targetAngle = new Vector2(transform.rotation.eulerAngles.y, 0f);
            }

            RenderManager.LevelOfDetail = 3;
            cc.m_targetPosition = cam.transform.position - camOffset;
            cc.m_targetSize = 20;

            ITerrain terrain = TerrainExtend.terrainManager;

            //Terrain collision
            Vector3 pos = transform.position;
            Mesh mesh = terrainGO.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                vertices[i].y = terrain.SampleTerrainHeight(pos.x + vertex.x, pos.z + vertex.z) - terrain.SampleTerrainHeight(pos.x, pos.z);
            }
            mesh.vertices = vertices;
            terrainGO.GetComponent<MeshCollider>().sharedMesh = mesh;
            terrainGO.transform.position = new Vector3(pos.x, terrain.SampleTerrainHeight(pos.x, pos.z), pos.z);

            //Road collision
            Mesh mesh2 = roadGO.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices2 = mesh.vertices;
            NetManager instance = Singleton<NetManager>.instance;
            NetSegment road = instance.m_segments.m_buffer[(int)roadID];
            for (int i = 0; i < vertices2.Length; i++)
            {
                Vector3 vertex = vertices2[i];
                vertices2[i].y = road.GetClosestPosition(new Vector3(pos.x + vertex.x, pos.y, pos.z + vertex.z)).y - road.GetClosestPosition(pos).y;
            }
            mesh2.vertices = vertices2;
            roadGO.GetComponent<MeshFilter>().mesh = mesh;
            roadGO.GetComponent<MeshCollider>().sharedMesh = mesh;
            roadGO.transform.position = new Vector3(pos.x, road.GetClosestPosition(pos).y, pos.z);

            FindCollidables();
            HandleInput();
        }
        void OnDestroy()
        {
            origCam.gameObject.SetActive(true);
            origCam.enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cc.m_freeCamera = false;
            cc.m_unlimitedCamera = false;
            Destroy(buildingGO);
            Destroy(this.gameObject);
        }

        public void SetCameraOffset(string situation)
        {
            switch (situation)
            {
                case "Walking": cameraOffset = new Vector3(0, 1, 0); break;
                case "Driving": cameraOffset = new Vector3(0, 5, -10); break;
            }
        }

        void SetBuilding(ushort newBuildingID)
        {
            Building building = bm.m_buildings.m_buffer[newBuildingID];
            oldBuildingID = newBuildingID;

            Vector3 buildingPos;
            Quaternion rot;
            building.CalculateMeshPosition(out buildingPos, out rot);
            buildingGO.transform.position = buildingPos;
            buildingGO.transform.rotation = Building.CalculateMeshRotation(building.m_angle);

            Mesh mesh = building.Info.m_lodMesh;
            buildingGO.GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        void FindCollidables()
        {
            ushort buildingID = bm.FindBuilding(transform.position, 32, ItemClass.Service.None, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
            if (buildingID != oldBuildingID)
            {
                SetBuilding(buildingID);
            }

            roadID = Find.FindRoad(transform.position);

            vehicleID = Find.FindVehicle(transform.position, 16, Vehicle.Flags.Created, Vehicle.Flags.Deleted);
            if (vehicleID > 0)
            {
                VehicleManager vm = Singleton<VehicleManager>.instance;
                Vehicle vehicle = vm.m_vehicles.m_buffer[(int)vehicleID];
                vehicleGO.transform.position = vehicle.GetSmoothPosition(vehicleID);
                vehicleGO.transform.rotation = vehicle.m_frame0.m_rotation;

                if (vehicleID != oldVehicleID)
                {
                    oldVehicleID = vehicleID;
                    Mesh mesh = vehicle.Info.m_mesh;
                    vehicleGO.GetComponent<BoxCollider>().size = mesh.bounds.size;
                }
            }

            parkedID = Find.FindParkedVehicle(transform.position, 16);
            if (parkedID != oldParkedID)
            {
                oldParkedID = parkedID;
                VehicleManager vm = Singleton<VehicleManager>.instance;
                VehicleParked vehicle = vm.m_parkedVehicles.m_buffer[(int)parkedID];
                parkedGO.transform.position = vehicle.m_position;
                parkedGO.transform.rotation = vehicle.m_rotation;
                parkedGO.GetComponent<MeshCollider>().sharedMesh = vehicle.Info.m_mesh;
            }
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Destroy(gameObject);

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                gameObject.GetComponent<MouseLook>().enabled = false;
                cam.GetComponent<MouseLook>().enabled = false;
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                gameObject.GetComponent<MouseLook>().enabled = true;
                cam.GetComponent<MouseLook>().enabled = true;
            }
            if (!Input.GetKey(KeyCode.LeftControl))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Joystick1Button0))
            {
                if (!driving && !following && GetComponent<PlayerWalk>().field.text == "Press E to enter vehicle")
                {
                    if (GetComponent<PlayerWalk>().hit.collider.name == "VehicleGO")
                    {
                        driving = true;
                        GameObject car = GetComponent<PlayerWalk>().hit.collider.gameObject;
                        Destroy(GetComponent<PlayerWalk>());
                        gameObject.AddComponent<VehicleControl>().car = car;
                        GetComponentsInChildren<MeshRenderer>()[1].enabled = false;
                        GetComponent<Rigidbody>().detectCollisions = false;
                        GetComponent<Rigidbody>().useGravity = false;
                        Physics.gravity = new Vector3(0, -200, 0);
                        SetCameraOffset("Driving");
                        vehicleGO.transform.position = new Vector3(0, 0, 0);
                    }
                    else
                    {
                        if (vehicleID > 0 || parkedID > 0)
                        {
                            VehicleManager vm = Singleton<VehicleManager>.instance;
                            if (vm.m_vehicles.m_buffer[(int)vehicleID].Info.GetService() == ItemClass.Service.PublicTransport)
                            {
                                following = true;
                                Destroy(GetComponent<PlayerWalk>());
                                gameObject.AddComponent<VehicleFollow>();
                                GetComponentsInChildren<MeshRenderer>()[1].enabled = false;
                                GetComponent<Rigidbody>().detectCollisions = false;
                                GetComponent<Rigidbody>().useGravity = false;
                                SetCameraOffset("Driving");
                            }
                            else {
                                driving = true;
                                Destroy(GetComponent<PlayerWalk>());
                                gameObject.AddComponent<VehicleControl>();
                                GetComponentsInChildren<MeshRenderer>()[1].enabled = false;
                                GetComponent<Rigidbody>().detectCollisions = false;
                                GetComponent<Rigidbody>().useGravity = false;
                                Physics.gravity = new Vector3(0, -200, 0);
                                SetCameraOffset("Driving");
                                vehicleGO.transform.position = new Vector3(0, 0, 0);
                            }
                        }
                    }    
                }
                else
                {
                    if (driving)
                    {
                        driving = false;
                        Destroy(GetComponent<VehicleControl>());
                        gameObject.AddComponent<PlayerWalk>();
                        Physics.gravity = new Vector3(0, -100, 0);
                        transform.position = new Vector3(transform.position.x, TerrainExtend.terrainManager.SampleTerrainHeight(transform.position.x, transform.position.z + 2) + 3, transform.position.z + 2);
                        transform.rotation = new Quaternion(0, 0, 0, 1);
                        GetComponent<Rigidbody>().useGravity = true;
                        GetComponent<Rigidbody>().detectCollisions = true;
                        GetComponentsInChildren<MeshRenderer>()[1].enabled = true;
                        SetCameraOffset("Walking");
                    }
                    if (following)
                    {
                        following = false;
                        Destroy(GetComponent<VehicleFollow>());
                        gameObject.AddComponent<PlayerWalk>();
                        transform.position = new Vector3(transform.position.x, TerrainExtend.terrainManager.SampleTerrainHeight(transform.position.x, transform.position.z) + 5, transform.position.z);
                        GetComponent<Rigidbody>().useGravity = true;
                        GetComponent<Rigidbody>().detectCollisions = true;
                        GetComponentsInChildren<MeshRenderer>()[1].enabled = true;
                        SetCameraOffset("Walking");
                    }
                }
            }
        }

        void InitCams()
        {
            origCam = Camera.main;
            origCam.enabled = false;

            cc = FindObjectOfType<CameraController>();
            cc.m_unlimitedCamera = true;

            cam = new GameObject("Player Cam").AddComponent<Camera>();
            cam.transform.parent = gameObject.transform;
            cam.tag = "MainCamera";
            cam.transform.localRotation = Quaternion.Euler(0, 0, 0);

            cam.hdr = true;
            cam.depth = origCam.depth + 1;
            cam.nearClipPlane = 1f;
            cam.cullingMask |= 1 << Singleton<RenderManager>.instance.lightSystem.m_lightLayer;

            cml = cam.gameObject.AddComponent<MouseLook>();
            cml.axes = MouseLook.RotationAxes.MouseY;

            gml = gameObject.AddComponent<MouseLook>();
            gml.axes = MouseLook.RotationAxes.MouseX;
        }
    }   

}