using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class MovementScript : MonoBehaviour
{
    private Camera _thisCamera;
    [SerializeField] private GameObject tubes;
    private bool _isBallSelected = false;
    private GameObject _selectedBall;
    private Rigidbody2D _rbBall;
    private GameObject _currentTube;
    private GameObject _previousBall;
    private int _countCompleteTubes;
    private static Scene _currentLevel;
    [SerializeField] private GameObject levelCompleteMenuUI;

    // The dictionary where the key is the name of the tube and the data is the balls in a stack
    private readonly Dictionary<string, Stack<GameObject>> _dictionaryBallTube = new Dictionary < string, Stack<GameObject>> { };


    // Start is called before the first frame update
    void Start()
    {
        _thisCamera = GetComponent<Camera>();

        foreach (Transform child in tubes.transform)
        {
            ballTracker(child.gameObject);
        }

        _currentLevel = SceneManager.GetActiveScene();
        _countCompleteTubes = 0;

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
                            ballSelector(go.gameObject); // get balls and select the top one then increase height of the top ball to the top of the tube
                            _currentTube = go.gameObject;
                        }
                    }
                }
            }
        }
    }
    
    // Adds the child objects of tubes and adds them to the dictionary
    void ballTracker(GameObject tube)
    {
        string keyName = tube.name;
        // Creates the stack that will be added to the dictionary
        Stack<GameObject> dictStack = new Stack<GameObject>();
        List<GameObject> tempList = new List<GameObject>();

        foreach (Transform child in tube.transform)
        {
            tempList.Add(child.gameObject);
        }
        //Orders the list by "height" 
        tempList = tempList.OrderBy(ball => ball.transform.localPosition.y).ToList();

        
        for(int i =0; i < tempList.Count; i++)
        {
            dictStack.Push(tempList[i]);
        }
        _dictionaryBallTube.Add(keyName, dictStack);
    }

    void ballSelector(GameObject tube)
    {
        string keyName = tube.name;
        Stack<GameObject> dictStack = _dictionaryBallTube[keyName];

        // if tube is empty move the ball
        if (dictStack.Count == 0)
        {
            if (_isBallSelected)
                HandleBallMove(tube, keyName);
            
            return;
        }
        //select top ball from tube
        _selectedBall = dictStack.Peek();

        // First ball picked up
        if (!_isBallSelected & _previousBall == null)
        {
            SelectBall(tube);
        }
        // Picking up new ball
        else if (!_isBallSelected & _previousBall != _selectedBall)
        {
            SelectBall(tube);
        }
        // Holding a new ball, selected different tube
        else if (_isBallSelected & _previousBall != _selectedBall)
        {
            HandleBallMove(tube, keyName);
        }
        // Selected a ball, selected same tube
        else if (_isBallSelected & _previousBall == _selectedBall)
        {
            DeselectBall(tube);
        }
        // shouldn't ever happen
        /*else if (!_isBallSelected & _previousBall == _selectedBall & _rbBall.simulated)
        {
            Debug.Log("Something has gone horribly wrong");
            SelectBall(tube);
        }*/
    }

    void SelectBall(GameObject tube)
    {
        Debug.Log($"Picking up {_selectedBall}");
        Vector2 selectedBallPos = _selectedBall.transform.position;
        _selectedBall.transform.position = new Vector2(selectedBallPos.x, tube.transform.position.y + 2);
        _rbBall = _selectedBall.GetComponent<Rigidbody2D>();
        _rbBall.simulated = false;
        _isBallSelected = true;
        _previousBall = _selectedBall;
    }

    void HandleBallMove(GameObject tube, string keyName)
    {
        if (_previousBall.CompareTag(_selectedBall.tag) & _dictionaryBallTube[keyName].Count < 4)
        {
            Debug.Log($"Moving {_previousBall} to {keyName}");
            _dictionaryBallTube[keyName].Push(_previousBall);
            _dictionaryBallTube[_currentTube.name].Pop();
            MoveBall(tube);
            _isBallSelected = false;
            _previousBall = _selectedBall;
            _selectedBall = null;
            if (_dictionaryBallTube[keyName].Count == 4)
            {
                FinChecker(tube, keyName);
            }
        }
        else
        {
            Debug.Log($"{_previousBall} not moved (different tag or full tube)");
            Debug.Log($" TAG CHECK: {_previousBall.CompareTag(_selectedBall.tag)} COUNT CHECK: {_dictionaryBallTube[_currentTube.name].Count}");
            _rbBall.simulated = true;
            _isBallSelected = false;
            ballSelector(tube);
            
        }
    }
    
    void MoveBall(GameObject tube)
    {
        _previousBall.transform.position = new Vector2(tube.transform.position.x, tube.transform.position.y + 2);
        _rbBall.simulated = true;
    }

    void DeselectBall(GameObject tube)
    {
        Debug.Log($"Deselecting {_selectedBall.name}");
        _rbBall.simulated = true;
        _isBallSelected = false;
        _previousBall = _selectedBall;
    }

    void FinChecker(GameObject tube, string keyName)
    {
        bool tagsMatch = false;
        GameObject lastBall = null;
        
        foreach (GameObject ball in _dictionaryBallTube[keyName])
        {
            if (lastBall == null)
            {
                lastBall = ball;
            }
            else if (ball.CompareTag(lastBall.tag))
            {
                tagsMatch = true;
                lastBall = ball;
                Debug.Log("tags match");
            }
            else if (lastBall != null & !ball.CompareTag(lastBall.tag))
            {
                tagsMatch = false;
                lastBall = ball;
                Debug.Log("tags don't match");
                break;
            }
        }

        if (!tagsMatch)
            return;
        
        
        Debug.Log("tags match and completed tubes increases");
        _countCompleteTubes++;
        
            
        if(_countCompleteTubes == _dictionaryBallTube.Count -2)
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
