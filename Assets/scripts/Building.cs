using UnityEngine;
using UnityEngine.EventSystems;

public class Building : MonoBehaviour {

    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;
    // Keys^

    int selected_building = -1;
    bool isObjectIndexChanged = false;
    //
    Vector3 mouse_pos;
    int dx, dz;
    bool obj_orientation = true;
    //
    Transform obj;
    MeshRenderer renderer_component;
    float obj_current_size_x;
    float obj_current_size_z;
    //obj data^

    Quaternion rotation;
    //buffers^

    BuildingSystem building_system;
    GameObject[] buildings;
    //
    void Start() 
    {
        building_system = new BuildingSystem((int)(this.transform.localScale.x + 0.5f)/10, (int)(this.transform.localScale.z + 0.5f)/10, 10);
        buildings = Resources.LoadAll<GameObject>("buildings");
    }
    //
    void Update() 
    {
        bool isAllowedPosition = false;
        if (obj != null)
        {
            bool rotateLeft = Input.GetKeyUp(rotateLeftKey);
            bool rotateRight = Input.GetKeyUp(rotateRightKey);
            if (rotateLeft ^ rotateRight)
            {
                obj.Rotate(0, (rotateRight) ? -90 : 90, 0, Space.Self);
                rotation = obj.transform.localRotation;
                //
                //float sin = Mathf.Sin(rotation.eulerAngles.y * Mathf.Deg2Rad);
                //float cos = Mathf.Cos(rotation.eulerAngles.y * Mathf.Deg2Rad);
                //sin_int = (int)(sin + (0.5f * ((sin < 0) ? -1 : 1)));
                //cos_int = (int)(cos + (0.5f * ((cos < 0) ? -1 : 1)));
                //
                obj_orientation = !obj_orientation;
                obj_current_size_x = (obj_orientation) ? obj.transform.localScale.x : obj.transform.localScale.z;
                obj_current_size_z = (obj_orientation) ? obj.transform.localScale.z : obj.transform.localScale.x;
            }
            //building rotation^

            mouse_pos.x = Input.mousePosition.x / Screen.width;
            mouse_pos.y = Input.mousePosition.y / Screen.height;
            Ray ray = Camera.main.ViewportPointToRay(mouse_pos);
            RaycastHit[] hit = Physics.RaycastAll(ray);
            for (int i = 0; i < hit.Length; ++i)
            {
                if (hit[i].transform.name == "building_plane")
                {
                    mouse_pos.x = (int)(hit[i].point.x + 0.5f) + obj_current_size_x/2; // + (obj_current_size_x % 2) / 2;
                    mouse_pos.y = obj.position.y;
                    mouse_pos.z = (int)(hit[i].point.z + 0.5f) + obj_current_size_z/2; // + (obj_current_size_z % 2) / 2;
                    //
                    obj.position = mouse_pos;
                    //
                    dx = (int)((int)(hit[i].point.x + 0.5f) - transform.position.x + transform.localScale.x/2);
                    dz = (int)((int)(hit[i].point.z + 0.5f) - transform.position.z + transform.localScale.z/2);
                    //
                    isAllowedPosition = building_system.CheckPosition(dx, dz, (int)(obj_current_size_x + 0.5f), (int)(obj_current_size_z + 0.5f));
                    renderer_component.material.color = (isAllowedPosition) ? Color.green : Color.red;
                    //
                    break;
                }
            }
            //set building position to mouse cursor^
        }

        bool case1 = selected_building > -1 && isObjectIndexChanged;
        bool case2 = Input.GetMouseButtonDown(0) && isAllowedPosition && EventSystem.current.IsPointerOverGameObject() == false;
        if (case1 || case2)
        {
            if (case2) Debug.Log(building_system.FillRect(selected_building, dx, dz, (int)(obj_current_size_x + 0.5f), (int)(obj_current_size_z + 0.5f)));
            else if (obj != null) Destroy(obj.gameObject);

            obj = Instantiate(buildings[selected_building]).transform;
            obj.transform.localRotation = rotation;
            renderer_component = obj.GetComponent<MeshRenderer>();
            //
            obj_current_size_x = (obj_orientation) ? obj.transform.localScale.x : obj.transform.localScale.z;
            obj_current_size_z = (obj_orientation) ? obj.transform.localScale.z : obj.transform.localScale.x;
            //
            isObjectIndexChanged = false;
            //creates new object anyway^
        }
        //change building object in hand or place building^
    }

    public void ChangeBuilding(int index)
    {
        selected_building = index;
        isObjectIndexChanged = true;
    }
    public void ClearBuilding()
    {
        selected_building = -1;
        Destroy(obj.gameObject);
    }
}

class BuildingSystem
{
    BuildingSector[,] sectors;
    struct BuildingSector {
        public bool isAvailable;
        public int[,] building_position;
    }
    int sector_size;
    //
    public BuildingSystem(int width, int height, int sector_size) 
    {
        this.sector_size = sector_size;
        sectors = new BuildingSector[width, height];
        for (int i = 0; i < sectors.GetLength(0); ++i) 
        {
            for (int k = 0; k < sectors.GetLength(1); ++k) 
            {
                sectors[i, k].isAvailable = true;
                sectors[i, k].building_position = new int[sector_size, sector_size];
                for (int ii = 0; ii < sectors[i, k].building_position.GetLength(0); ++ii)
                {
                    for (int jj = 0; jj < sectors[i, k].building_position.GetLength(1); ++jj)
                    {
                        sectors[i, k].building_position[ii, jj] = -1;
                    }
                }
            }
        }
    }
    public bool CheckPosition(int x, int y, int width, int height)
    {
        bool isAvailable = true;
        try
        {
            for (int ix = x; ix < x + width; ++ix)
            {
                for (int iy = y; iy < y + height; ++iy)
                {
                    if (sectors[ix / sector_size, iy / sector_size].isAvailable == false) return false;
                    isAvailable &= sectors[ix / sector_size, iy / sector_size].building_position[ix % sector_size, iy % sector_size] == -1;
                }
            }
        }
        catch (System.Exception exc) { return false; }
        return isAvailable;
    }
    public int FillRect(int value, int x, int y, int width = 1, int height = 1) 
    {
        //if (CheckPosition(x, y, width, height) == false) return;
        int counter = 0;
        for (int ix = x; ix < x + width; ++ix)
        {
            for (int iy = y; iy < y + height; ++iy)
            {
                sectors[ix / sector_size, iy / sector_size].building_position[ix % sector_size, iy % sector_size] = value;
                ++counter;
            }
        }
        return counter;
    }
}