using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; 

public class Movement : MonoBehaviour
{
    
    Camera thisCamera;
    public GameObject Tubes;
    private bool isBallSelected = false;
    public GameObject selBall;
    public Rigidbody2D ballFreeze;
    GameObject currGO;
    GameObject currBall;
    int numFin;//number of tubes that have been finished (all balls the same colour inside
    Scene currentLevel;
    

    Dictionary<string, List<GameObject>> ballTube = new Dictionary<string, List<GameObject>> { };

    // Start is called before the first frame update
    void Start()
    {
        thisCamera = GetComponent<Camera>();
        
        foreach (Transform child in Tubes.transform)
        {
            ballTracker(child.gameObject);
        }
         currentLevel = SceneManager.GetActiveScene();
        numFin = 0;

        
    }

    // Update is called once per frame
    void Update()
    {
        if (!ButtonScript.gameIsPaused)
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
    }


    
    void ballTracker(GameObject Tube) // when called this function looks at all child objects of a Tube and adds them to the dictionary - it's only really used to create the dictionary to start the level
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

            if (ballTube[keyName].Count == 0) //this runs when an existing tube becomes empty and you're trying to put a ball in the empty tube
            {
                Debug.Log("Tube is empty: ");

                ballTube[keyName].Add(selBall);

                ballFreeze = selBall.GetComponent<Rigidbody2D>();

                selBall.transform.position = new Vector2(Tube.transform.position.x, Tube.transform.position.y + 2);
                ballFreeze.simulated = true;

                ballTube[currGO.name].Remove(selBall);

                Debug.Log(selBall + " has been removed from " + currGO + " and added to " + keyName);
                currBall = selBall;
                selBall = null;
                isBallSelected = false;
                Debug.Log("ball selected: " + isBallSelected + "Ball simulated: " + selBall.GetComponent<Rigidbody2D>().simulated);
                //issue here where when ball is moved into the empty tube, in unity it doesn't become simulated even though here it does

            }
            else //this runs if a tube is not empty
            {
                selBall = dicList[0];
            }


            if (isBallSelected == false & currBall == null) // this runs if no ball is selected and there have been no balls previously selected (first move)
            {
                Debug.Log("Valid tube. No ball selected. currBall = null");
                selBallPos = selBall.transform.position;
                selBall.transform.position = new Vector2(selBallPos.x, Tube.transform.position.y + 2);
                ballFreeze = selBall.GetComponent<Rigidbody2D>();
                ballFreeze.simulated = false;

                isBallSelected = true;
                currBall = selBall;
            }
            else if (isBallSelected == false & currBall != selBall & currBall != null) // this runs when a no ball is selected, but a ball has previously been used (e.g. just moved a ball and are selecting a new ball
            {
                Debug.Log("Valid tube. No ball selected. ");
                selBallPos = selBall.transform.position;
                selBall.transform.position = new Vector2(selBallPos.x, Tube.transform.position.y + 2);
                ballFreeze = selBall.GetComponent<Rigidbody2D>();
                ballFreeze.simulated = false;

                isBallSelected = true;
                currBall = selBall;

            }
            else if (isBallSelected == true & currBall != selBall & currBall != null)// this runs when a ball is selected and you click another tube
            {
                if (currBall.tag == selBall.tag & ballTube[keyName].Count < 4) // this checks if the balls have the same tag(colour) and the tube is not full, in order to move the ball
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
                    Debug.Log(ballTube[keyName].Count);
                    if (ballTube[keyName].Count == 4)
                    {
                        finChecker(Tube);
                    }
                }
                else //this runs if the balls are different colours or the tube is full (drops selected ball and re-runs to select new ball)
                {
                    Debug.Log("Valid Tube. Ball Selected. Different tag or tube is already full");
                    ballFreeze.simulated = true;
                    isBallSelected = false;

                    ballSelector(Tube);
                }
            }
            else if ( isBallSelected == false & currBall == selBall & ballFreeze.simulated == true) //this causes ball dropping issue
            {
                Debug.Log("When does this run?");
                selBallPos = selBall.transform.position;
                selBall.transform.position = new Vector2(selBallPos.x, Tube.transform.position.y + 2);
                ballFreeze = selBall.GetComponent<Rigidbody2D>();
                ballFreeze.simulated = false;

                isBallSelected = true;
                currBall = selBall;
            }
            else if (isBallSelected == true & currBall == selBall) // this runs if you select the same ball, and drops the ball, unselecting it
            {
                Debug.Log("Valid tube. Ball Selected. Same ball");


                ballFreeze.simulated = true;
                isBallSelected = false;
                currBall = selBall;
            }
        }

        else if (isBallSelected == true) //this sections runs if a ball has been selected and the tube is not in the dictionary (moving a ball to an empty tube at the start of the level
        {
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

    public void finChecker(GameObject Tube)
    {
        string keyName = Tube.name;
        bool tagsMatch = false;
        GameObject lastBall = null;
        if (numFin != ballTube.Count - 2)
        {
            foreach (GameObject ball in ballTube[keyName])
            {
                if (lastBall == null)
                {
                    lastBall = ball;
                }
                else if (lastBall != null & ball.tag == lastBall.tag)
                {
                    tagsMatch = true;
                    lastBall = ball;
                    Debug.Log("tags match");
                }
                else if (lastBall != null & ball.tag != lastBall.tag)
                {
                    tagsMatch = false;
                    lastBall = ball;
                    Debug.Log("tags don't match");
                    break;
                }
            }
            if (tagsMatch == true)
            {
                Debug.Log("tags match and numfin increase");
                numFin++;
            }
        }
        if(numFin == ballTube.Count -2)
        {
            Debug.Log("Loading next level");
            StartCoroutine(LoadNextLevel());
        }
    }
    IEnumerator LoadNextLevel()
    {
        int levelNum = currentLevel.buildIndex;
        int buildIndex = SceneUtility.GetBuildIndexByScenePath($"Level {levelNum}"); //this works because the buildIndex is always 1 above the current level name e.g. level 1 is build index 2
        if (levelNum > 10)
        {
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(1);
        }
        else if (buildIndex > 0)
        {
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(buildIndex);
        }
        else
        {
            yield return new WaitForSeconds(1);
            Debug.LogError("There was an error loading that scene");
            SceneManager.LoadScene(0);
        }
    }
}