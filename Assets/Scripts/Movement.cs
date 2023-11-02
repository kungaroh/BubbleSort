using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private bool isBallSelected = false;
    public GameObject selBall;
    public Rigidbody2D ballFreeze;
    GameObject currGO;
    GameObject currBall;

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
        if (Input.GetMouseButtonDown(0))
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
                        currGO = go.gameObject;
                    }
                }
            }
        }
    }


    GameObject ball;
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

                ballTube[keyName] = ballTube[keyName].OrderBy(y => Vector2.Distance(Tube.transform.localPosition, y.transform.localPosition)).ToList();
            }
            else
            {
                ballTube[keyName].Add(child.gameObject);

                ballTube[keyName] = ballTube[keyName].OrderBy( y => Vector2.Distance(Tube.transform.localPosition, y.transform.localPosition)).ToList();
            }
        }//then sort them based on height

    }

    

    void ballSelector(GameObject Tube)
    {
        string keyName = Tube.name;
        List<GameObject> dicList;
        Vector2 selBallPos;




        if (ballTube.ContainsKey(keyName) == true)
        {
            dicList = ballTube[keyName];

            if (ballTube[keyName].Count <= 0 )
            {
                //this causes a memory leak somehow xx
                ballTube[keyName].Add(selBall);

                selBall.transform.position = new Vector2(Tube.transform.position.x, Tube.transform.position.y + 2);
                ballFreeze.simulated = true;
                ballTube[currGO.name].Remove(selBall);
                Debug.Log(selBall + " has been removed from " + currGO + " and added to " + keyName);

                isBallSelected = false;
            }
            else
            {
                selBall = dicList[0];
            }

            if (isBallSelected == false &  currBall!= selBall & currBall != null)
            {
                Debug.Log("Valid tube. No ball selected. ");
                selBallPos = selBall.transform.position;
                selBall.transform.position = new Vector2(selBallPos.x, Tube.transform.position.y + 2);
                ballFreeze = selBall.GetComponent<Rigidbody2D>();
                ballFreeze.simulated = false;

                isBallSelected = true;
                currBall = selBall;
                
            }
            else if (isBallSelected == true & currBall != selBall & currBall != null)
            {
                if (currBall.tag == selBall.tag)
                {
                    Debug.Log("Valid tube. ball selected. Balls has same tag ");
                    ballTube[keyName].Add(currBall);
                    ballTube[currGO.name].Remove(currBall);

                    ballTube[keyName] = ballTube[keyName].OrderBy(y => Vector2.Distance(Tube.transform.localPosition, y.transform.localPosition)).ToList();

                    Debug.Log("Tube added to list");

                    currBall.transform.position = new Vector2(Tube.transform.position.x, Tube.transform.position.y + 2);
                    ballFreeze.simulated = true;
                    Debug.Log(currBall + " has been removed from " + currGO + " and added to " + keyName);
                    isBallSelected = false;
                    currBall = selBall;
                }
                else
                {
                    Debug.Log("Valid Tube. Ball Selected. Different tag");
                    ballFreeze.simulated = true;
                    isBallSelected = false;

                    ballSelector(Tube);
                }
            }
            else if (isBallSelected == true & currBall == selBall)
            {
                Debug.Log("Valid tube. Ball Selected. Same ball");
                ballFreeze.simulated = true;
                isBallSelected = false;
            }
            else if(isBallSelected == false & currBall == null)
            {
                Debug.Log("Valid tube. No ball selected. currBall = null");
                selBallPos = selBall.transform.position;
                selBall.transform.position = new Vector2(selBallPos.x, Tube.transform.position.y + 2);
                ballFreeze = selBall.GetComponent<Rigidbody2D>();
                ballFreeze.simulated = false;

                isBallSelected = true;
                currBall = selBall;
            }
            else
            {
                Debug.Log("Valid Tube. Ball Selected. Different balls");
                ballFreeze.simulated = true;
                isBallSelected = false;

                ballSelector(Tube);
            }
        }
        

        else
        {    //this assumes a ball has already been selected
            //if a tube isn't in ballTube dictionary, then add it to the dictionary with the currently selected ball, and move the ball there
            Debug.LogWarning("Tube does not exist");
            Debug.Log(selBall);
            dicList = new List<GameObject>();
            dicList.Add(selBall);
            ballTube.Add(keyName, dicList);

            ballTube[keyName] = ballTube[keyName].OrderBy(y => Vector2.Distance(Tube.transform.localPosition, y.transform.localPosition)).ToList();

            Debug.Log("Tube added to list");

            selBall.transform.position = new Vector2(Tube.transform.position.x, Tube.transform.position.y + 2);
            ballFreeze.simulated = true;
            ballTube[currGO.name].Remove(selBall);
            Debug.Log(selBall + " has been removed from " + currGO + " and added to " + keyName);

            isBallSelected = false;
        }
        
    }
}