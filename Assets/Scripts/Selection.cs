using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{
    public bool isSelected = false;
    public Material deselected;
    public Material selected;


    private void Update()
    {
        Highlight();
    }

    private void Highlight()
    {
        if (isSelected)
        {
            gameObject.GetComponent<MeshRenderer>().material = selected;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = deselected;
        }
    }
}
