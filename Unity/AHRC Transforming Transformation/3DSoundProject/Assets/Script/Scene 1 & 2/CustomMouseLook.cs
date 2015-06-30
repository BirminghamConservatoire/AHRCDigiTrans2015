using UnityEngine;
using System.Collections;

public class CustomMouseLook : MonoBehaviour
{


    public float sensitivityX = 2.0F;
    public float sensitivityY = 2.0F;

    public float sensitivityZoom = 20.0f;

    private float fov;

    private float rotx;
    private float roty;

    private float initRotx;
    private float initRoty;

    void Start()
    {
        fov = transform.gameObject.GetComponent<Camera>().fieldOfView;
    }

    void OnEnable()
    {
        initRotx = transform.localRotation.eulerAngles.x;
        initRoty = transform.localRotation.eulerAngles.y;

        rotx = transform.localRotation.eulerAngles.x;
        roty = transform.localRotation.eulerAngles.y;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            rotx += -1 * Input.GetAxis("Mouse Y") * sensitivityY;
            rotx = Mathf.Clamp(rotx, initRotx - 50, initRotx + 25);

            roty += Input.GetAxis("Mouse X") * sensitivityX;
            //roty = Mathf.Clamp(roty, initRoty - 35, initRoty + 35);

            transform.localRotation = Quaternion.Euler(rotx, roty, 0.0f);

        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            fov += Input.GetAxis("Mouse ScrollWheel") * sensitivityZoom;
            fov = Mathf.Clamp(fov, 5, 90);
            transform.gameObject.GetComponent<Camera>().fieldOfView = fov;
        }
    }
}
