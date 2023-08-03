using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewBottle : IObject, IAction
{
    public static event EventHandler OnCompleteBottle;
    public static event EventHandler OnUndoCompleteBottle;
    private GameObject _object;
    private List<NewIcon> _iconInBottle;
    private bool _isComplete;
    private float _startWidth, _endWidth, _startHeight, _endHeight;
    private bool _isSelected;
    private int _numEmptyIcon;
    private int _numMaxIcon;
    private int _currentIcon;
    private bool _completeAction;
    public NewBottle(GameObject newObject, Vector3 positon)
    {
        _object = newObject;
        _object.transform.position = positon;
        _iconInBottle = new List<NewIcon>();
        _isSelected = false;
        _numMaxIcon = 4;
        _numEmptyIcon = _numMaxIcon;
        _object.transform.Find("Lid").gameObject.SetActive(false);
        SetArea();     
    }

    public void Action()
    {
        if (Vector3.Distance(_object.transform.Find("Lid").transform.position, _object.transform.Find("LidClosedPosition").transform.position) > 0.1f && _completeAction)
        {
            _object.transform.Find("Lid").transform.position = Vector3.MoveTowards(_object.transform.Find("Lid").transform.position, _object.transform.Find("LidClosedPosition").transform.position, 5 * Time.deltaTime);
            if (Vector3.Distance(_object.transform.Find("Lid").transform.position, _object.transform.Find("LidClosedPosition").transform.position) < 0.1f)
            {
                _object.transform.Find("Lid").transform.position = _object.transform.Find("LidClosedPosition").transform.position;
                _completeAction = false;
            }
        }
    }

    public void SetUp()
    {
        ClearBottle();
        _object.transform.Find("Lid").gameObject.SetActive(false);
        _object.transform.Find("Lid").transform.localPosition = _object.transform.Find("LidOpenPosition").transform.localPosition;
        _object.transform.Find("Selected").gameObject.SetActive(false);
        _numEmptyIcon = _numMaxIcon;
        _isSelected = false;
        _isComplete = false;
    }

    public void FillBottle(NewIcon icon)
    {
        _iconInBottle.Add(icon);
        icon.GetIconGO().transform.position = _object.transform.GetChild(_numMaxIcon - _numEmptyIcon).transform.position;
        icon.SetTargetPosition(icon.GetIconGO().transform.position);
        icon.GetIconGO().transform.parent = _object.transform;
        _numEmptyIcon--;
        _currentIcon++;
    }

    public void ClearBottle()
    {
        _iconInBottle.Clear();
    }

    private void SetArea()
    {
        _startWidth = _object.transform.position.x  - _object.GetComponent<SpriteRenderer>().bounds.size.x / 2;
        _endWidth = _object.transform.position.x + _object.GetComponent<SpriteRenderer>().bounds.size.x / 2;
        _startHeight = _object.transform.position.y - _object.GetComponent<SpriteRenderer>().bounds.size.y / 2;
        _endHeight = _object.transform.position.y + _object.GetComponent<SpriteRenderer>().bounds.size.y / 2;
    }
    public bool ActivateState { 
        get => _object.activeSelf; 
        set => _object.SetActive(value); 
    }

    public void RemoveLast()
    {
        _iconInBottle.RemoveAt(_iconInBottle.Count - 1);
        _numEmptyIcon++;

    }

    public bool AddIconToBottle(NewIcon icon)
    {
        if(GetLastIconInBottle() == null || GetLastIconInBottle()._iconVisual == icon._iconVisual && _iconInBottle.Count < _numMaxIcon)
        {
            _iconInBottle.Add(icon);
            icon.SetTargetPosition(_object.transform.GetChild(_numMaxIcon - _numEmptyIcon).transform.position);
            icon.Moving();
            icon.GetIconGO().transform.parent = _object.transform;
            _numEmptyIcon--;
            _currentIcon++;
            return true;
        } else
        {
            return false;
        }
    }

    public NewIcon GetLastIconInBottle()
    {
        return _iconInBottle.Count != 0 ? _iconInBottle.Last() : null;
    }

    public void MoveIcon(NewBottle selectedBottleB)
    {
        if (_isComplete || selectedBottleB._isComplete) return;
        if (GetLastIconInBottle() == null) return;
        for (int i = 0; i < _currentIcon; i++)
        {
            if (selectedBottleB.GetLastIconInBottle() == null || GetLastIconInBottle()._iconVisual == selectedBottleB.GetLastIconInBottle()._iconVisual)
            {
                bool isAdd = selectedBottleB.AddIconToBottle(GetLastIconInBottle());
                if (isAdd)
                {
                    RemoveLast();
                }
            }
        }
        _currentIcon = _iconInBottle.Count;
        _object.transform.Find("Selected").gameObject.SetActive(false);
        _isSelected = false;
    }

    public bool IsEmptyBottle()
    {
        return _iconInBottle.Count == 0;
    }
    public bool IsFullBottle()
    {
        return _iconInBottle.Count == _numMaxIcon;
    }

    public void CompleteBottle()
    {
        
        if(_iconInBottle.Count == _numMaxIcon)
        {
            _isComplete = true;
            for(int i = 1; i < _iconInBottle.Count; i++)
            {
                if (_iconInBottle[i]._iconVisual != _iconInBottle[0]._iconVisual)
                {
                    _object.transform.Find("Lid").gameObject.SetActive(false);
                    _isComplete = false;
                    break;
                }
            }
            if(_isComplete)
            {    
                OnCompleteBottle?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public bool IsCompleteBottle()
    {
        return _isComplete;
    }
    public bool SelectedBottle()
    {   
        if(!_isSelected)
        {
            _object.transform.Find("Selected").gameObject.SetActive(true);
            _isSelected = true;
        }
        else if (_isSelected)
        {
            _object.transform.Find("Selected").gameObject.SetActive(false);
            _isSelected = false;
        }

        return _isSelected;
    }
    public GameObject GetBottleGO()
    {
        return _object;
    }
     public void SettingSprite(Sprite sprite)
    {
        _object.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public int CurrentIconInBottle()
    {
        return _currentIcon;
    }

    public void UpdateCurrentIcon()
    {
        _currentIcon = _iconInBottle.Count;
    }

    public List<NewIcon> GetIconsInBottle()
    {
        return _iconInBottle;
    }
    public void UndoCompleteCheck()
    {
        if(_isComplete)
        {
            if (_iconInBottle.Count != _numMaxIcon)
            {
                _object.transform.Find("Lid").gameObject.SetActive(false);
                _object.transform.Find("Lid").transform.localPosition = _object.transform.Find("LidOpenPosition").transform.localPosition;
                _isComplete = false;
                OnUndoCompleteBottle?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public void CompleteAction()
    {
        if (_isComplete) {
            _completeAction = true;
            _object.transform.Find("Lid").gameObject.SetActive(true);
        }  
    }

    public (float startWidth, float endWidth, float startHeight, float endHeight) GetBottleArea()
    {
        return (_startWidth, _endWidth, _startHeight, _endHeight);
    }

    
}
