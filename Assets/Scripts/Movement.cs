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

    Dictionary<string, List<GameObject>> ballTube = new Dictionary<string, List<GameObject>> { };

    // Start is called before the first frame update
    void Start()
    {
        thisCamera = GetComponent<Camera>();
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
                        ballSelector(go.gameObject);//get balls and select the top one then increase height of the top ball to the top of the tube
                    }
                }
            }
        }
    }

    void ballTracker(GameObject Tube) // when called this function should track where all the balls are
    {
        string keyName = Tube.name;
        List<GameObject> dicList;
        
        foreach (Transform child in Tube.transform)
        {
            if (!ballTube.TryGetValue(keyName, out dicList))
            {
                dicList = new List<GameObject>();
                dicList.Add(child.gameObject);
                ballTube.Add(keyName, dicList);
            }
            else
            {
                ballTube[keyName].Add(child.gameObject);
            }
        }
        
        

        //detect all collisions, identify the balls and add them to an array, then sort them based on height
    }

    void ballSelector(GameObject Tube)
    {
        string keyName = Tube.name;
        List<GameObject> dicList = ballTube[keyName];
        Debug.Log(keyName);
        int j = 0;
        foreach (GameObject i in dicList)
        {
            Debug.Log(dicList[j]);
            j++;
        }
    }
}

