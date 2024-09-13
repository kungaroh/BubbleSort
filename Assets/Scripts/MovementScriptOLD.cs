using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MovementScriptOld : MonoBehaviour
{
    
    Camera _thisCamera;
    [FormerlySerializedAs("Tubes")] [SerializeField] private GameObject tubes;
    private bool _isBallSelected = false;
    private GameObject _selectedBall;
    private Rigidbody2D _rbBall;
    GameObject currGO;
    GameObject currBall;
    int _countFin;//number of tubes that have been finished (all balls the same colour inside)
    private static Scene _currentLevel;
    [FormerlySerializedAs("finishedMenuUI")] [SerializeField] private GameObject levelCompleteMenuUI;
    

    private readonly Dictionary<string, List<GameObject>> _dictBallTube = new Dictionary<string, List<GameObject>> { };

    // Start is called before the first frame update
    void Start()
    {
        _thisCamera = GetComponent<Camera>();
        
        foreach (Transform child in tubes.transform)
        {
            ballTracker(child.gameObject);
        }
        _currentLevel = SceneManager.GetActiveScene();
        _countFin = 0;
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
            if (!_dictBallTube.TryGetValue(keyName, out dicList))
            {
                dicList = new List<GameObject>();
                dicList.Add(child.gameObject);
                _dictBallTube.Add(keyName, dicList);

                _dictBallTube[keyName] = _dictBallTube[keyName].OrderBy(y => Vector2.Distance(Tube.transform.localPosition, y.transform.localPosition)).ToList();
            }
            else
            {
                _dictBallTube[keyName].Add(child.gameObject);

                _dictBallTube[keyName] = _dictBallTube[keyName].OrderBy( y => Vector2.Distance(Tube.transform.localPosition, y.transform.localPosition)).ToList();
            }
        }//then sort them based on height

    }



    void ballSelector(GameObject Tube)
    {
        string keyName = Tube.name;
        List<GameObject> dicList;
        Vector2 selBallPos;

        if (_dictBallTube.ContainsKey(keyName) == true)
        {
            dicList = _dictBallTube[keyName];

            if (_dictBallTube[keyName].Count == 0) //this runs when an existing tube becomes empty and you're trying to put a ball in the empty tube
            {
                Debug.Log("Tube is empty: ");

                _dictBallTube[keyName].Add(_selectedBall);

                _rbBall = _selectedBall.GetComponent<Rigidbody2D>();

                _selectedBall.transform.position = new Vector2(Tube.transform.position.x, Tube.transform.position.y + 2);
                _rbBall.simulated = true;

                _dictBallTube[currGO.name].Remove(_selectedBall);

                Debug.Log(_selectedBall + " has been removed from " + currGO + " and added to " + keyName);
                currBall = _selectedBall;
                Debug.Log("ball selected: " + _isBallSelected + "Ball simulated: " + _selectedBall.GetComponent<Rigidbody2D>().simulated);
                _selectedBall = null;
                _isBallSelected = false;
                //issue here where when ball is moved into the empty tube, in unity it doesn't become simulated even though here it does

            }
            else //this runs if a tube is not empty
            {
                _selectedBall = dicList[0];
            }


            if (_isBallSelected == false & currBall == null) // this runs if no ball is selected and there have been no balls previously selected (first move)
            {
                Debug.Log("Valid tube. No ball selected. currBall = null");
                selBallPos = _selectedBall.transform.position;
                _selectedBall.transform.position = new Vector2(selBallPos.x, Tube.transform.position.y + 2);
                _rbBall = _selectedBall.GetComponent<Rigidbody2D>();
                _rbBall.simulated = false;

                _isBallSelected = true;
                currBall = _selectedBall;
            }
            else if (_isBallSelected == false & currBall != _selectedBall & currBall != null) // this runs when a no ball is selected, but a ball has previously been used (e.g. just moved a ball and are selecting a new ball
            {
                
                Debug.Log("Valid tube. No ball selected. ");
                Debug.Log($"selected Ball: {_selectedBall.name} current ball: {currBall.name}");
                selBallPos = _selectedBall.transform.position;
                _selectedBall.transform.position = new Vector2(selBallPos.x, Tube.transform.position.y + 2);
                _rbBall = _selectedBall.GetComponent<Rigidbody2D>();
                _rbBall.simulated = false;

                _isBallSelected = true;
                currBall = _selectedBall;

            }
            else if (_isBallSelected == true & currBall != _selectedBall & currBall != null)// this runs when a ball is selected and you click another tube
            {
                if (currBall.tag == _selectedBall.tag & _dictBallTube[keyName].Count < 4) // this checks if the balls have the same tag(colour) and the tube is not full, in order to move the ball
                {
                    Debug.Log("Valid tube. ball selected. Balls has same tag ");
                    _dictBallTube[keyName].Add(currBall);
                    _dictBallTube[currGO.name].Remove(currBall);

                    _dictBallTube[keyName] = _dictBallTube[keyName].OrderBy(y => Vector2.Distance(Tube.transform.localPosition, y.transform.localPosition)).ToList();

                    Debug.Log("Tube added to list");

                    currBall.transform.position = new Vector2(Tube.transform.position.x, Tube.transform.position.y + 2);
                    _rbBall.simulated = true;
                    Debug.Log(currBall + " has been removed from " + currGO + " and added to " + keyName);
                    _isBallSelected = false;
                    currBall = _selectedBall;
                    Debug.Log(_dictBallTube[keyName].Count);
                    if (_dictBallTube[keyName].Count == 4)
                    {
                        Debug.Log($"{keyName} is full ");
                        finChecker(Tube);
                    }
                }
                else //this runs if the balls are different colours or the tube is full (drops selected ball and re-runs to select new ball)
                {
                    Debug.Log("Valid Tube. Ball Selected. Different tag or tube is already full");
                    _rbBall.simulated = true;
                    _isBallSelected = false;

                    ballSelector(Tube);
                }
            }
            else if ( _isBallSelected == false & currBall == _selectedBall & _rbBall.simulated == true) // this runs when ball is placed and selected ball has not been reset this causes ball dropping issue
            {
                Debug.Log("When does this run?");
                selBallPos = _selectedBall.transform.position;
                _selectedBall.transform.position = new Vector2(selBallPos.x, Tube.transform.position.y + 2);
                _rbBall = _selectedBall.GetComponent<Rigidbody2D>();
                _rbBall.simulated = false;

                _isBallSelected = true;
                currBall = _selectedBall;
            }
            else if (_isBallSelected == true & currBall == _selectedBall) // this runs if you select the same ball, and drops the ball, unselecting it
            {
                Debug.Log("Valid tube. Ball Selected. Same ball");


                _rbBall.simulated = true;
                _isBallSelected = false;
                currBall = _selectedBall;
            }
        }

        else if (_isBallSelected == true) //this sections runs if a ball has been selected and the tube is not in the dictionary (moving a ball to an empty tube at the start of the level)
        {
            Debug.LogWarning("Tube does not exist");
            Debug.Log(_selectedBall);
            dicList = new List<GameObject>();
            dicList.Add(_selectedBall);
            _dictBallTube.Add(keyName, dicList);

            _dictBallTube[keyName] = _dictBallTube[keyName].OrderBy(y => Vector2.Distance(Tube.transform.localPosition, y.transform.localPosition)).ToList();

            Debug.Log("Tube added to list");

            _selectedBall.transform.position = new Vector2(Tube.transform.position.x, Tube.transform.position.y + 2);
            _rbBall.simulated = true;

            _dictBallTube[currGO.name].Remove(_selectedBall);
            Debug.Log(_selectedBall + " has been removed from " + currGO + " and added to " + keyName);

            _isBallSelected = false;
        }
    }

    public void finChecker(GameObject Tube)
    {
        string keyName = Tube.name;
        bool tagsMatch = false;
        GameObject lastBall = null;
        if (_countFin != _dictBallTube.Count - 2)
        {
            foreach (GameObject ball in _dictBallTube[keyName])
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
                _countFin++;
            }
        }
        if(_countFin == _dictBallTube.Count -2)
        {
            Debug.Log("Level Complete");
            levelCompleteMenuUI.SetActive(true);
            ButtonScript.gameIsPaused = true;
        }
    }
    public static IEnumerator LoadNextLevel()
    {
        Debug.Log("LoadNextLevel Called");
        int levelNum = _currentLevel.buildIndex;
        int buildIndex = SceneUtility.GetBuildIndexByScenePath($"Level {levelNum}"); //this works because the buildIndex is always 1 above the current level name e.g. level 1 is build index 2
        
        if (levelNum > 10)
        {
            yield return new WaitForSeconds(1);
            Debug.Log("Game Complete");
            SceneManager.LoadScene(1);
        }
        else if (buildIndex > 0)
        {
            yield return new WaitForSeconds(1);
            Debug.Log("loading level " + buildIndex);
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