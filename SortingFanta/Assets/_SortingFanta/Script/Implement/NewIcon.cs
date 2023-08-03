using System;
using UnityEngine;

public class NewIcon: IObject, IAction
{
    public static event EventHandler OnIconEndMove;
    private GameObject _object;
    private Vector3 _targetPosition;
    private bool _isMoving;
    public enum IconVisual
    {
        BallVisualA = 0,
        BallVisualB = 1,
        BallVisualC = 2,
        BallVisualD = 3,
        BallVisualE = 4,
        BallVisualF = 5,
        BallVisualG = 6,
    }

    public IconVisual _iconVisual;

    

    public NewIcon(GameObject newObject, int iconVisual = 0)
    {
        _object = newObject;
        _iconVisual = (IconVisual)iconVisual;
    }

    public void Action()
    {
        if (_isMoving)
        {
            _object.transform.position = Vector3.MoveTowards(_object.transform.position, _targetPosition, 5 * Time.deltaTime);
            if (Vector3.Distance(_object.transform.position, _targetPosition) < 0.1f)
            {
                _object.transform.position = _targetPosition;
                _isMoving = false;
                OnIconEndMove?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void SetUp(int ballVisual = 0)
    {
        _iconVisual = (IconVisual)ballVisual;
    }

    public void Moving()
    {
        _isMoving = true;
    }

    public bool CompareVisualType(IconVisual ballVisual)
    {
        return _iconVisual == ballVisual;
    }

    public void SetTargetPosition(Vector3 newTargetPosition)
    {
        _targetPosition = newTargetPosition;
    }

    public bool ActivateState
    {
        get => _object.activeSelf;
        set => _object.SetActive(value);
    }

    public void SettingSprite(Sprite sprite)
    {
        _object.GetComponent<SpriteRenderer>().sprite = sprite;
    }
    public GameObject GetIconGO()
    {
        return _object;
    }
}
