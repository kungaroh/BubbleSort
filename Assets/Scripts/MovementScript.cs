using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public enum State
{
    NoBall,
    HoldingBall,
    MovingBall,
    TubeFull,
    InvalidMove
}

public class MovementScript : MonoBehaviour
{
    [SerializeField] private GameObject tubes;
    private GameObject _selectedBall;
    private Rigidbody2D _rbBall;
    private GameObject _currentTube;
    private GameObject _newTube;
    private GameObject _previousBall;
    private GameObject _selectedTube;
    private int _countCompleteTubes;
    private static Scene _currentLevel;
    private int _winTubes;
    [SerializeField] private GameObject levelCompleteMenuUI;

    private State _currentState = State.NoBall;

    // key is the name of the tube and the data is stack of balls
    private readonly Dictionary<string, Stack<GameObject>> _dictionaryBallTube;


    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in tubes.transform)
        {
            BallTracker(child.gameObject);
        }

        _currentLevel = SceneManager.GetActiveScene();
        _countCompleteTubes = 0;
        _winTubes = _dictionaryBallTube.Count - 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (!ButtonScript.gameIsPaused)
        {

            switch (_currentState)
            {
                case State.NoBall:
                    HandleNoBall();
                    break;
                case State.HoldingBall:
                    HandleHoldingBall();
                    break;
                case State.MovingBall:
                    HandleMovingBall();
                    break;
                case State.TubeFull:
                    HandleTubeFull();
                    break;
                case State.InvalidMove:
                    HandleInvalidMove();
                    break;
            }
        }
    }
    
    // Adds the balls in the tubes in the correct place in the dictionary
    void BallTracker(GameObject tube)
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

    
    // State Handlers
    void HandleNoBall()
    {
        _currentTube = null;
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
                        SelectBall(go.gameObject);
                        _currentTube = go.gameObject;
                        _currentState = State.HoldingBall;
                    }
                }
                
            }
        }
    }

    void HandleHoldingBall()
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
                        _selectedTube = go.gameObject;
                    }
                }
                
            }
        }
        
        if (_selectedTube == null )
        {
            return;
        }

        if (_currentTube == _selectedTube)
        {
            _selectedTube = null;
            DeselectBall();
            _currentState = State.NoBall;
            return;
        }

        _currentState = IsValidMove(_selectedTube) ? State.MovingBall : State.InvalidMove;
    }

    void HandleMovingBall()
    {
        string newKey = _selectedTube.name;
        string keyName = _currentTube.name;
        Debug.Log(_selectedBall.name);
        _dictionaryBallTube[newKey].Push(_selectedBall);
        _dictionaryBallTube[keyName].Pop();
        MoveBall(_selectedTube);
        if (_dictionaryBallTube[newKey].Count == 4)
        {
            _currentState = State.TubeFull;
        }
        else
        {
            _selectedTube = null;
            _currentState = State.NoBall;
            
        }
    }

    void HandleTubeFull()
    {
        bool tagsMatch = false;
        GameObject lastBall = null;
        
        foreach (GameObject ball in _dictionaryBallTube[_selectedTube.name])
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
            else if (!ball.CompareTag(lastBall.tag))
            {
                tagsMatch = false;
                Debug.Log("tags don't match");
                break;
            }
            
        }

        if (!tagsMatch)
        {
            _selectedTube = null;
            _currentState = State.NoBall;
            return;
        }
        
        Debug.Log("tags match and completed tubes increases");
        _countCompleteTubes++;
        
        if(_countCompleteTubes == _winTubes)
        {
            Debug.Log("Level Complete");
            levelCompleteMenuUI.SetActive(true);
            ButtonScript.gameIsPaused = true;
        }

        _selectedTube = null;
        _currentState = State.NoBall;
    }

    void HandleInvalidMove()
    {
        Debug.Log("Checking move");
        DeselectBall();
        SelectBall(_selectedTube);
        _currentTube = _selectedTube;
        _selectedTube = null;
        _currentState = State.HoldingBall;
    }
    
    // Ball Management
    void SelectBall(GameObject tube)
    {
        string keyName = tube.name;
        if (_dictionaryBallTube[keyName].Count > 0)
        {
            _selectedBall = _dictionaryBallTube[keyName].Peek();
        }
        Debug.Log($"Picking up {_selectedBall}");
        Vector2 selectedBallPos = _selectedBall.transform.position;
        _selectedBall.transform.position = new Vector2(selectedBallPos.x, tube.transform.position.y + 2);
        _rbBall = _selectedBall.GetComponent<Rigidbody2D>();
        _rbBall.simulated = false;
    }

    void DeselectBall()
    {
        Debug.Log($"Deselecting {_selectedBall.name}");
        _rbBall.simulated = true;
    } 
    
    void MoveBall(GameObject tube)
    {
        _selectedBall.transform.position = new Vector2(tube.transform.position.x, tube.transform.position.y + 2);
        _rbBall.simulated = true;
    }
    
    // Utility Methods
    bool IsValidMove(GameObject tube)
    {
        
        if(_dictionaryBallTube[tube.name].Count < 1)
        {
            return true;
        }    
        if (_dictionaryBallTube[tube.name].Count == 4)
        {
            Debug.Log("Tube is full");
            return false;
        }
        GameObject topBall = _dictionaryBallTube[tube.name].Peek();
        if (!topBall.CompareTag(_selectedBall.tag))
        {
            Debug.Log("different coloured ball");
            return false;
        }

        return true;
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