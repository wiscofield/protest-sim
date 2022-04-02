using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Command : MonoBehaviour
{
    const int SELECTABLE_MASK = 1 << 3;
    const int TERRAIN_MASK = 1 << 6;

    private HashSet<GameObject> selected;
    private Camera cam;

    private PlayerInput input;
    private InputAction pointerPos;

    private void Awake()
    {
        cam = Camera.main;
        input = gameObject.GetComponent<PlayerInput>();
        pointerPos = input.currentActionMap.FindAction("PointPosition");
        selected = new HashSet<GameObject>();

        input.onActionTriggered += Click;
    }

    private void Update()
    {
        Ray r = cam.ScreenPointToRay(pointerPos.ReadValue<Vector2>());
        Debug.DrawLine(r.origin, r.origin + (r.direction * 100), Color.red);
    }

    private void Click(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (context.action.name)
            {
                case "Click":
                    Vector2 mousePos = pointerPos.ReadValue<Vector2>();
                    Ray pointRay = cam.ScreenPointToRay(mousePos);
                    Debug.DrawLine(pointRay.origin, pointRay.origin + (pointRay.direction * 100), Color.green, 1.0f);


                    if (selected.Count == 0)
                    {
                        SingleSelect(pointRay);
                    }
                    else
                    {
                        SetDest(pointRay);
                    }
                    break;

                case "Clear":
                    WipeSelected();
                    break;
            }
        }
    }

    private void SingleSelect(Ray r)
    {
        RaycastHit hit;

        if(Physics.Raycast(r, out hit, Mathf.Infinity, SELECTABLE_MASK))
        {
            GameObject prot = hit.collider.gameObject;
            selected.Add(prot);
            prot.GetComponent<Selection>().isSelected = true;
        }
    }

    private void SetDest(Ray r)
    {
        RaycastHit hit;

        if (Physics.Raycast(r, out hit, Mathf.Infinity, TERRAIN_MASK))
        {
            Vector3 target = hit.point;

            if (selected.Count == 1)
            {
                foreach (GameObject unit in selected)
                {
                    unit.GetComponent<NavMeshAgent>().SetDestination(target);
                }
                WipeSelected();
            }
        }
    }

    private void WipeSelected()
    {
        foreach (GameObject prot in selected)
        {
            prot.GetComponent<Selection>().isSelected = false;    
        }

        selected.Clear();
    }
}