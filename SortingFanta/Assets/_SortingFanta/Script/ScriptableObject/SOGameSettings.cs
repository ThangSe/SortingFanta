using UnityEngine;
[CreateAssetMenu(fileName = "Game Settings SO", menuName = "ScriptableObjects/GameSettingRefsSO")]
public class SOGameSettings : ScriptableObject
{
    public SOFloatVariable
        countdownToStartTime, countdownToStartTimeMax,
        currentScore, winningScore,
        turnPlayLeft, turnPlayMax,
        timePlayingCurrent, timePlayingMax,
        numberSameIcon, numberIconType,
        numberBottle,
        numberIconInBottle,
        numberEmptyBottle,
        undoTurnCurrent, undoTurnMax,
        hintTurnCurrent, hintTurnMax;
    public Vector3[] bottlePosition;
}
