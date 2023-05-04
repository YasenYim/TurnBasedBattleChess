using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    Camera cam;

    [SerializeField]
    Transform pointer;

    public static InputController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(mousePos);

        bool ok = Input.GetMouseButtonDown(0);
        bool cancel = Input.GetMouseButtonDown(1);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Cha", "BG")))
        {
            //Debug.DrawLine(ray.origin, hit.point);
            pointer.gameObject.SetActive(true);
            pointer.transform.position = hit.transform.position;

            Pos pos = new Pos(hit.transform.position);
            GameManager.Instance.OnTouch(pos, ok, cancel);      // ☆
        }
        else
        {
            pointer.gameObject.SetActive(false);
        }
    }
}
