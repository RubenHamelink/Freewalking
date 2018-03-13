using ColossalFramework.UI;
using UnityEngine;

namespace Freewalking
{
    class PlayerWalk : MonoBehaviour
    {
        float speed;
        float normalSpeed;
        float rotSpeed = 2;
        Quaternion oldRot;
        GameObject model;
        public UITextField field;
        public RaycastHit hit;

        void Start()
        {
            normalSpeed = 800;
            speed = normalSpeed;
            model = GetComponentsInChildren<Transform>()[1].gameObject;
            oldRot = transform.rotation;
            UIView v = UIView.GetAView();
            field = v.AddUIComponent(typeof(UITextField)) as UITextField;
            field.width = 200;
            field.name = "InfoField";
        }

        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 8 - GetComponent<PlayerControl>().cameraZoom))
            {
                if (hit.collider.name == "Vehicle" || hit.collider.name == "Parked vehicle" || hit.collider.name == "VehicleGO")
                    field.text = "Press E to enter vehicle";
            }
            else
            {
                field.text = "";
            }
        }

        void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.LeftShift))
                speed = normalSpeed * 2;
            else
                speed = normalSpeed;

            Vector3 v = Vector3.zero;
            v.y = GetComponent<Rigidbody>().velocity.y;

            if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
            {
                v += transform.forward * (speed * Time.deltaTime) * SettingsPanel.speedMod * Input.GetAxis("Vertical");
                Vector3 rot = transform.rotation.eulerAngles;
                rot += new Vector3(0, rotSpeed * Input.GetAxis("Horizontal"), 0);
                transform.rotation = Quaternion.Euler(rot);
            }
            else {
                if (Input.GetKey(KeyCode.W))
                {
                    v += transform.forward * (speed * Time.deltaTime) * SettingsPanel.speedMod;
                    oldRot = transform.rotation;
                }
                if (Input.GetKey(KeyCode.S))
                    v -= transform.forward * (speed * Time.deltaTime) * SettingsPanel.speedMod;
                if (Input.GetKey(KeyCode.D))
                    v += transform.right * (speed * Time.deltaTime) * SettingsPanel.speedMod;
                if (Input.GetKey(KeyCode.A))
                    v -= transform.right * (speed * Time.deltaTime) * SettingsPanel.speedMod;
            }
            if (Input.GetKeyDown(KeyCode.Space) && Physics.Raycast(new Ray(transform.position, -transform.up), GetComponent<CapsuleCollider>().height / 2 + 1))
            {
                v += transform.up * 20;
            }

            model.transform.rotation = oldRot;
            GetComponent<Rigidbody>().velocity = v;
        }

        void OnDestroy()
        {
            UIView v = UIView.GetAView();
            Destroy(v.FindUIComponent("InfoField"));
        }
    }
}
