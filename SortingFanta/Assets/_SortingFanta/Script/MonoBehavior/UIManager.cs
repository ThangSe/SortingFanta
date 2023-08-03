using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text _gameEndText;
    [SerializeField] private Image _timeFill;
    [SerializeField] private GameObject _backgroundPanel, _homePage, _tutorialPage, _basePage, _gamePage, _gameEndPage;
    [Header("Audio")]
    [SerializeField] private AudioSource _audioGamePlay, _audioGameEnd;
    [SerializeField] private FloatReference _currentScore, _winningScore;

    private void Start()
    {
        MainManager.Instance.OnStateChanged += MainManager_OnStateChanged;
    }

    private void Update()
    {
        if(MainManager.Instance.IsGamePlayingState())
        {
            _timeFill.fillAmount = MainManager.Instance.TimePlayingNormalize();
        }
    }

    private void MainManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (MainManager.Instance.IsHomeState())
        {
            _homePage.SetActive(true);
            _gameEndPage.SetActive(false);
            _audioGameEnd.volume = 0f;
        }
        if (MainManager.Instance.IsWaitingToStartState())
        {
            _timeFill.fillAmount = 0f;
            _audioGameEnd.volume = 0f;
            _tutorialPage.SetActive(true);
            _homePage.SetActive(false);
            _gameEndPage.SetActive(false);
        }
        if (MainManager.Instance.IsCountdownToStartState())
        {
            _audioGamePlay.time = 0f;
            if (!_audioGamePlay.isPlaying) _audioGamePlay.Play();
            _audioGamePlay.volume = 1f;
            _backgroundPanel.SetActive(false);
            _tutorialPage.SetActive(false);
            _gamePage.SetActive(true);
        }
        if (MainManager.Instance.IsGamePlayingState())
        {

        }
        if (MainManager.Instance.IsGameEndState())
        {
            _audioGamePlay.volume = 0f;
            _audioGameEnd.time = 0f;
            if (!_audioGameEnd.isPlaying) _audioGameEnd.Play();
            _audioGameEnd.volume = 1f;
            _gamePage.SetActive(false);
            _backgroundPanel.SetActive(true);
            _gameEndPage.SetActive(true);
            if(_currentScore.Value == _winningScore.Value)
            {
                _gameEndText.text = "YOU WIN";
            } else
            {
                _gameEndText.text = "YOU LOSE";
            }
        }
    }
}
