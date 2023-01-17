using UnityEngine;

public class CameraController : MonoBehaviour {

    public float scale = 0.007f;
    //
    bool isDragKeyPressed = false;
    Vector3 mousePositionLast;
    Vector3 mousePositionDelta;
    //
	void Start () {
		
	}
	//
	void Update () {
        if (Input.GetMouseButton(1))
        {
            if (isDragKeyPressed == false) mousePositionLast = Input.mousePosition;
            isDragKeyPressed = true;
            mousePositionDelta = Input.mousePosition - mousePositionLast;
            mousePositionLast = Input.mousePosition;
            transform.position += Vector3.right * mousePositionDelta.x * scale + Vector3.forward * mousePositionDelta.y * scale;
        }
        else { isDragKeyPressed = false; }
	}
}
