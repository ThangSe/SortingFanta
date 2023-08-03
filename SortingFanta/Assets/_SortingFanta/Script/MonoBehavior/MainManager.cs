using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }

    public event EventHandler OnStateChanged;

    [SerializeField] private SOGameSettings _gameSettingRefsSO;


    [Header("Button")]
    [SerializeField] private Button _tutorialButton, _playButton, _replayButton, _undoButton, _hintButton;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioPoint, _audioNotMove;

    private Vector3 _mouseWorldPosition;
    [SerializeField] GameObject _bottleGO, _iconGO, _glassGO;
    [SerializeField] private UIManager _uiManager;
    private enum State
    {
        Home,
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        HintMove,
        GameEnd,

    }

    private State _state;
    private List<NewBottle> _bottleList;
    private List<NewIcon> _iconList;
    private List<LastMove> _lastMoveList;
    private NewBottle _firstSelectedBottle;
    private NewBottle _secondSelectedBottle;
    private bool _isSelectBottle;
    private bool _isFirstTime = true;
    private System.Random _random;

    [SerializeField] private Image[] _iconImgsRef;
    [SerializeField] private Image _glassImgRef;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        
        _bottleList = new List<NewBottle>();
        _iconList = new List<NewIcon>();
        _lastMoveList = new List<LastMove>();
        _state = State.WaitingToStart;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Start()
    {
        LoadGlassVisual();
        SetUpBottleFirstTime();
        SetUpDefault();
        _isFirstTime = false;
        _tutorialButton.onClick.AddListener(() =>
        {
            if (_gameSettingRefsSO.turnPlayLeft.value > 0)
            {
                _state = State.WaitingToStart;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        });
        _replayButton.onClick.AddListener(() =>
        {
            SetUpDefault();
            if (_gameSettingRefsSO.turnPlayLeft.value > 0)
            {
                _state = State.WaitingToStart;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _state = State.Home;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        });
        _playButton.onClick.AddListener(() =>
        {
            _state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        });
        _undoButton.onClick.AddListener(() =>
        {
            UndoAction();
        });
        _hintButton.onClick.AddListener(() =>
        {
            if(_gameSettingRefsSO.hintTurnCurrent.value > 0)
            {
                HintAction();
                _gameSettingRefsSO.hintTurnCurrent.value--;
            }
        });
        NewBottle.OnCompleteBottle += NewBottle_OnCompleteBottle;
        NewBottle.OnUndoCompleteBottle += NewBottle_OnUndoCompleteBottle;
        NewIcon.OnIconEndMove += NewIcon_OnIconEndMove;
    }

    public void LoadGlassVisual()
    {
        _glassGO.GetComponent<SpriteRenderer>().sprite = _glassImgRef.sprite;
        _glassGO.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = _glassImgRef.sprite;
        _glassGO.GetComponentInChildren<SpriteMask>().sprite = _glassImgRef.sprite;
    }

    private void NewIcon_OnIconEndMove(object sender, EventArgs e)
    {
        _secondSelectedBottle.CompleteAction();
    }

    private void NewBottle_OnUndoCompleteBottle(object sender, EventArgs e)
    {
        _gameSettingRefsSO.currentScore.value--;
    }

    private void NewBottle_OnCompleteBottle(object sender, EventArgs e)
    {
        _gameSettingRefsSO.currentScore.value++;
        _audioPoint.Play();
        if(_gameSettingRefsSO.currentScore.value == _gameSettingRefsSO.winningScore.value)
        {
            _state = State.GameEnd;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public void SetUpBottleFirstTime()
    {
        Debug.Log("Here");
        for (int i = 0; i < _gameSettingRefsSO.numberIconType.value; i++)
        {
            for (int j = 0; j < _gameSettingRefsSO.numberSameIcon.value; j++)
            {
                _iconList.Add(new NewIcon(Instantiate(_iconGO), i));
                _iconList[_iconList.Count - 1].SettingSprite(_iconImgsRef[i].sprite);
            }
        }
        _random = new System.Random();
        List<int> randomnumbers = Enumerable.Range(0, _iconList.Count).OrderBy(x => _random.Next()).Take(_iconList.Count).ToList();
        for (int i = 0; i < _gameSettingRefsSO.numberBottle.value; i++)
        {
            _bottleList.Add(new NewBottle(Instantiate(_bottleGO), _gameSettingRefsSO.bottlePosition[i]));
            if (i < _gameSettingRefsSO.numberBottle.value - _gameSettingRefsSO.numberEmptyBottle.value)
            {
                for (int j = 0; j < _gameSettingRefsSO.numberIconInBottle.value; j++)
                {
                    _bottleList[_bottleList.Count - 1].FillBottle(_iconList[randomnumbers[j + i * (int)_gameSettingRefsSO.numberIconInBottle.value]]);
                }
            }
        }
    }
    private void SetUpBottle()
    {
        List<int> randomnumbers = Enumerable.Range(0, _iconList.Count).OrderBy(x => _random.Next()).Take(_iconList.Count).ToList();
        for (int i = 0; i < _bottleList.Count; i++)
        {
            _bottleList[i].SetUp();
            if(i < _gameSettingRefsSO.numberBottle.value - _gameSettingRefsSO.numberEmptyBottle.value)
            {
                for (int j = 0; j < _gameSettingRefsSO.numberIconInBottle.value; j++)
                {
                    _bottleList[i].FillBottle(_iconList[randomnumbers[j + i * (int)_gameSettingRefsSO.numberIconInBottle.value]]);
                }
            }
        }
    }

    private void SetUpDefault()
    {
        _gameSettingRefsSO.turnPlayLeft.value = _gameSettingRefsSO.turnPlayMax.value;
        _gameSettingRefsSO.undoTurnCurrent.value = _gameSettingRefsSO.undoTurnMax.value;
        _gameSettingRefsSO.countdownToStartTime.value = _gameSettingRefsSO.countdownToStartTimeMax.value;
        _gameSettingRefsSO.hintTurnCurrent.value = _gameSettingRefsSO.hintTurnMax.value;
        _gameSettingRefsSO.timePlayingCurrent.value = _gameSettingRefsSO.timePlayingMax.value;
        _gameSettingRefsSO.currentScore.value = 0;
        if (!_isFirstTime) SetUpBottle();
    }

    private void AddLastMove(NewBottle lastBottleMoveFrom, NewBottle lastBottleMoveTo)
    {
        LastMove lastMove = new LastMove(lastBottleMoveFrom, lastBottleMoveTo);
        if (_lastMoveList.Count < 10)
        {
            _lastMoveList.Add(lastMove);
        } else
        {
            _lastMoveList.RemoveAt(0);
            _lastMoveList.Add(lastMove);
        }
    }
    private void UndoAction()
    {
        if(_lastMoveList.Count > 0 && _gameSettingRefsSO.undoTurnCurrent.value > 0)
        {
            NewBottle lastBottleFrom = _lastMoveList[_lastMoveList.Count - 1].bottleMoveFrom;
            NewBottle lastBottleTo = _lastMoveList[_lastMoveList.Count - 1].bottleMoveTo;
            for (int i = 0; i < _bottleList.Count; i++)
            {
                if (lastBottleFrom.GetBottleGO().GetInstanceID() == _bottleList[i].GetBottleGO().GetInstanceID())
                {
                    for (int j = 0; j < _bottleList.Count; j++)
                    {
                        if(lastBottleTo.GetBottleGO().GetInstanceID() == _bottleList[j].GetBottleGO().GetInstanceID())
                        {
                            int numIconMove = _lastMoveList[_lastMoveList.Count - 1].numIconBeforeMove - _bottleList[i].GetIconsInBottle().Count;
                            for (int k = 0; k < numIconMove; k++)
                            {
                                lastBottleFrom.FillBottle(lastBottleTo.GetLastIconInBottle());
                                lastBottleTo.RemoveLast();
                                lastBottleTo.UpdateCurrentIcon();
                            }
                            lastBottleTo.UndoCompleteCheck();
                            _lastMoveList.RemoveAt(_lastMoveList.Count - 1);
                            _gameSettingRefsSO.undoTurnCurrent.value--;
                            break;
                        }     
                    }         
                    break;
                }
            }   
        }   
    }
    private void HintAction()
    {
        bool hintSuccess = false;
        for (int i = 0; i < _bottleList.Count; i++)
        {
            for (int j = 0; j < _bottleList.Count; j++)
            {
                if (_bottleList[i].IsEmptyBottle()) break;
                if ((_bottleList[j].GetLastIconInBottle() == null || _bottleList[i].GetLastIconInBottle()._iconVisual == _bottleList[j].GetLastIconInBottle()._iconVisual) &&
                    !_bottleList[i].IsCompleteBottle() &&
                    !_bottleList[j].IsCompleteBottle() &&
                    _bottleList[j].GetBottleGO().GetInstanceID() != _bottleList[i].GetBottleGO().GetInstanceID() &&
                    !_bottleList[j].IsFullBottle())
                {
                    AddLastMove(_bottleList[i], _bottleList[j]);
                    _bottleList[i].MoveIcon(_bottleList[j]);
                    _secondSelectedBottle = _bottleList[j];
                    _bottleList[j].CompleteBottle();
                    hintSuccess = true;
                    break;
                }
            }
            if (hintSuccess) break;
        }
    }
    private void Update()
    {
        switch (_state)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                _gameSettingRefsSO.countdownToStartTime.value -= Time.deltaTime;
                if (_gameSettingRefsSO.countdownToStartTime.value < 0f)
                {
                    _state = State.GamePlaying;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                _gameSettingRefsSO.timePlayingCurrent.value -= Time.deltaTime;
                MoveIconLogic();
                UpadateAction();
                if (_gameSettingRefsSO.timePlayingCurrent.value < 0f)
                {
                    _state = State.GameEnd;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameEnd:
                break;
        }
    }

    private void MoveIconLogic()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            _mouseWorldPosition = GetMouseWorldPosition();
            if (!_isSelectBottle)
            {
                for (int i = 0; i < _bottleList.Count; i++)
                {
                    if (_mouseWorldPosition.x < _bottleList[i].GetBottleArea().endWidth &&
                        _mouseWorldPosition.x > _bottleList[i].GetBottleArea().startWidth &&
                        _mouseWorldPosition.y < _bottleList[i].GetBottleArea().endHeight &&
                        _mouseWorldPosition.y > _bottleList[i].GetBottleArea().startHeight)
                    {
                        if ((!_bottleList[i].IsEmptyBottle() && !_bottleList[i].IsFullBottle()) ||
                            (_bottleList[i].IsFullBottle() && !_bottleList[i].IsCompleteBottle()))
                        {
                            _bottleList[i].SelectedBottle();
                            _firstSelectedBottle = _bottleList[i];
                            _isSelectBottle = true;
                        }
                    }
                    if (_isSelectBottle) break;
                }

            }
            else if (_isSelectBottle)
            {
                for (int i = 0; i < _bottleList.Count; i++)
                {
                    if (_mouseWorldPosition.x < _bottleList[i].GetBottleArea().endWidth &&
                        _mouseWorldPosition.x > _bottleList[i].GetBottleArea().startWidth &&
                        _mouseWorldPosition.y < _bottleList[i].GetBottleArea().endHeight &&
                        _mouseWorldPosition.y > _bottleList[i].GetBottleArea().startHeight)
                    {
                        if (_bottleList[i].GetBottleGO().GetInstanceID() == _firstSelectedBottle.GetBottleGO().GetInstanceID() ||
                            _bottleList[i].IsFullBottle() ||
                            (!_bottleList[i].IsEmptyBottle() && _bottleList[i].GetLastIconInBottle()._iconVisual != _firstSelectedBottle.GetLastIconInBottle()._iconVisual))
                        {
                            _firstSelectedBottle.SelectedBottle();
                            _secondSelectedBottle = _bottleList[i];
                            _audioNotMove.Play();
                            StartCoroutine(BottleShake(_secondSelectedBottle));
                            _isSelectBottle = false;
                        }
                        else if ((_bottleList[i].GetLastIconInBottle() == null || _bottleList[i].GetLastIconInBottle()._iconVisual == _firstSelectedBottle.GetLastIconInBottle()._iconVisual))
                        {
                            _secondSelectedBottle = _bottleList[i];
                            AddLastMove(_firstSelectedBottle, _secondSelectedBottle);
                            _firstSelectedBottle.MoveIcon(_secondSelectedBottle);
                            _secondSelectedBottle.CompleteBottle();
                            _isSelectBottle = false;
                        }
                        break;
                    }
                }
            }
        }
    }

    private IEnumerator BottleShake(NewBottle bottleWithShake)
    {
        Vector3 originalPosition = bottleWithShake.GetBottleGO().transform.position;
        float duration = .15f;
        float elapsed = 0f;
        float shakeAmount = .1f;
        while(elapsed < duration)
        {
            float x = UnityEngine.Random.Range(originalPosition.x - shakeAmount, originalPosition.x + shakeAmount);
            float y = UnityEngine.Random.Range(originalPosition.y - shakeAmount, originalPosition.y + shakeAmount);
            bottleWithShake.GetBottleGO().transform.position = new Vector3(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bottleWithShake.GetBottleGO().transform.position = originalPosition;
    }

    private void UpadateAction()
    {
        for(int i = 0; i < _iconList.Count; i++)
        {
            _iconList[i].Action();
        }
        for(int i = 0; i < _bottleList.Count; i++)
        {
            _bottleList[i].Action();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    public float TimePlayingNormalize()
    {
        return 1 - (_gameSettingRefsSO.timePlayingCurrent.value / _gameSettingRefsSO.timePlayingMax.value);
    }

    public bool IsHomeState()
    {
        return _state == State.Home;
    }

    public bool IsWaitingToStartState()
    {
        return _state == State.WaitingToStart;
    }

    public bool IsCountdownToStartState()
    {
        return _state == State.CountdownToStart;
    }

    public bool IsGamePlayingState()
    {
        return _state == State.GamePlaying;
    }

    public bool IsGameEndState()
    {
        return _state == State.GameEnd;
    }
    
}
public class LastMove
{
    public NewBottle bottleMoveFrom;
    public NewBottle bottleMoveTo;
    public int numIconBeforeMove;
    public LastMove(NewBottle _lastBottleFrom, NewBottle _lastBottleTo)
    {
        bottleMoveFrom = _lastBottleFrom;
        bottleMoveTo = _lastBottleTo;
        numIconBeforeMove = _lastBottleFrom.GetIconsInBottle().Count;
    }
}
