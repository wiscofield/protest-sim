using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/**
 * https://www.youtube.com/watch?v=OL1QgwaDsqo
 */
public class Command : MonoBehaviour
{
    const int SELECTABLE_MASK = 1 << 3;
    const int TERRAIN_MASK = 1 << 6;

    // Set of all selected protesters
    private HashSet<GameObject> selected;

    // Camera
    private Camera cam;

    // Mouse positions
    private Vector3 p1;
    private Vector3 p2;

    // For multi-select
    private float multi_threshold = 40;
    private bool multi;
    private MeshCollider selectionBox;
    private Mesh selectionMesh;
    private Vector2[] corners;
    private Vector3[] verts;
    private Vector3[] vecs;
    private float height = 2;



    private void Awake()
    {
        cam = Camera.main;
        selected = new HashSet<GameObject>();
    }

    private void Update()
    {
        // Clear
        if (Input.GetMouseButtonDown(1))
        {
            WipeSelected();
        }

        // Down
        if (Input.GetMouseButtonDown(0))
        {
            p1 = Input.mousePosition;
        }

        // Hold
        if (Input.GetMouseButton(0))
        {
            if((p1 - Input.mousePosition).magnitude > multi_threshold)
            {
                multi = true;
            }
        }

        // Up
        if (Input.GetMouseButtonUp(0))
        {
            if (multi)
            {
                p2 = Input.mousePosition;

                corners = GetCorners(p1, p2);
                verts = new Vector3[4];
                vecs = new Vector3[4];

                for(int i = 0; i < corners.Length; i++)
                {
                    Vector2 corner = corners[i];
                    Ray r = cam.ScreenPointToRay(corner);
                    RaycastHit hit;

                    if (Physics.Raycast(r, out hit, Mathf.Infinity, TERRAIN_MASK))
                    {
                        verts[i] = hit.point;
                        vecs[i] = new Vector3(hit.point.x, hit.point.y + height, hit.point.z);
                    }
                    else
                    {
                        Debug.Log("Missed, fix this");
                    }
                }

                selectionMesh = GenerateSelectionMesh(verts, vecs);
                selectionBox = gameObject.AddComponent<MeshCollider>();
                selectionBox.sharedMesh = selectionMesh;
                selectionBox.convex = true;
                selectionBox.isTrigger = true;

                Destroy(selectionBox, 0.02f);

                multi = false;
            }
            else
            {
                Ray r = cam.ScreenPointToRay(p1);
                RaycastHit hit;

                if (Physics.Raycast(r, out hit, Mathf.Infinity, SELECTABLE_MASK))
                {
                    GameObject go = hit.collider.gameObject;

                    if (!Select(go))
                    {
                        Deselect(go);
                    }
                }
                else if (selected.Count > 0 && Physics.Raycast(r, out hit, Mathf.Infinity, TERRAIN_MASK))
                {
                    Vector3 dest = hit.point;

                    foreach (GameObject go in selected)
                    {
                        SetDest(go, dest);
                    }
                    WipeSelected();
                }
            }
        }
    }

    private bool Select(GameObject go)
    {
        if (!selected.Contains(go))
        {
            selected.Add(go);
            go.GetComponent<Selection>().isSelected = true;
            return true;
        }
        return false;
    }

    private void Deselect(GameObject go)
    {
        if (selected.Contains(go))
        {
            selected.Remove(go);
            go.GetComponent<Selection>().isSelected = false;
        }
    }

    private void WipeSelected()
    {
        foreach (GameObject go in selected)
        {
            go.GetComponent<Selection>().isSelected = false;
        }
        selected.Clear();
    }

    private void SetDest(GameObject go, Vector3 target)
    {
        go.GetComponent<NavMeshAgent>().SetDestination(target);
    }

    private Vector2[] GetCorners(Vector2 p1, Vector2 p2)
    {
        Vector2 topLeft;
        Vector2 topRight;
        Vector2 botLeft;
        Vector2 botRight;

        float minX = Mathf.Min(p1.x, p2.x);
        float maxX = Mathf.Max(p1.x, p2.x);
        float minY = Mathf.Min(p1.y, p2.y);
        float maxY = Mathf.Max(p1.y, p2.y);

        topLeft = new Vector2(minX, maxY);
        topRight = new Vector2(maxX, maxY);
        botLeft = new Vector2(minX, minY);
        botRight = new Vector2(maxX, minY);

        Vector2[] v = {topLeft, topRight, botLeft, botRight};

        return v;
    }
    Mesh GenerateSelectionMesh(Vector3[] corners, Vector3[] vecs)
    {
        Vector3[] verts = new Vector3[8];
        int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 }; //map the tris of our cube

        for (int i = 0; i < 4; i++)
        {
            verts[i] = corners[i];
        }

        for (int j = 4; j < 8; j++)
        {
            verts[j] = corners[j - 4] + vecs[j - 4];
        }

        Mesh selectionMesh = new Mesh();
        selectionMesh.vertices = verts;
        selectionMesh.triangles = tris;

        return selectionMesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Protester"))
        {
            Select(other.gameObject);
        }
    }
}