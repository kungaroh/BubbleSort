using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Movement : MonoBehaviour
{
    /* PLAN:
     * onClick(Tube) = select tube & find out what top ball is 
     * onClick(secondTube) tube.full ? if yes cannot move ball if no is ball colour matching? yes = move ball no = don't move ball
     */
    Camera thisCamera;
    public GameObject Tubes;

    // Start is called before the first frame update
    void Start()
    {
        thisCamera = GetComponent<Camera>();
        Debug.Log(Tubes.transform.childCount);
        foreach (Transform child in Tubes.transform)
        {
            ballTracker(child.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButton(0))
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0)
            {
                foreach (var go in raycastResults)
                {
                    if (go.gameObject.name.StartsWith("Tube"))
                    {
                        ballSelector();//get balls and select the top one then increase height of the top ball to the top of the tube
                    }
                }
            }
        }
    }

    void ballTracker(GameObject Tube) // when called this function should track where all the balls are
    {
        //detect all collisions, identify the balls and add them to an array, then sort them based on height
    }

    void ballSelector()
    {
        //select ball
    }
}

