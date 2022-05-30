using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BilliardGame.EventHandlers;
using BilliardGame.Managers;

namespace BilliardGame.UIControllers
{
    public class GameUIScreen : MonoBehaviour
    {
        [SerializeField]
        private GameObject _pausePageGo;

        [SerializeField]
        private VerticalLayoutGroup _playerGridGroup;

        [SerializeField]
        private GameObject _playerUITemplate;

        [SerializeField]
        private GameObject _loadingTextGo;

        [SerializeField]
        private GameObject _quitButton;

        [SerializeField]
        private GameObject _PauseButton;

        [SerializeField]
        private GameObject _playButton;

        [SerializeField]
        private Text _gameComplete;

        private void Start()
        {
            // subscribe
            EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInput);
            EventManager.Subscribe(typeof(ScoreUpdateEvent).Name, OnScoreUpdate);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnPlayerTurnChanged);
        }

        private void OnDestroy()
        {
            // unsubscribe
            EventManager.Unsubscribe(typeof(GameInputEvent).Name, OnGameInput);
            EventManager.Unsubscribe(typeof(ScoreUpdateEvent).Name, OnScoreUpdate);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnPlayerTurnChanged);
        }

        private void OnScoreUpdate(object sender, IGameEvent gameEvent)
        {
            Player player = (Player)sender;
            PlayerUIController[] playerUIControllers = _playerGridGroup.GetComponentsInChildren<PlayerUIController>();
            if (playerUIControllers != null && playerUIControllers.Length > 0)
            {
                PlayerUIController playerUIController = playerUIControllers.FirstOrDefault(p => p.name.Equals(player.Name));
                playerUIController.NameNScore.text = player.Name + " " + player.Score;
            }
            
        }

        private void OnPlayerTurnChanged(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;

            if (gameStateEvent.GameState == GameStateEvent.State.Complete)
            {
                string winningText = string.Empty;

                winningText = "Game Complete, Winner is " + GameManager.Instance.Winner;
                

                GameManager.Instance.ChangeGameState(GameManager.GameState.Practise);

                _gameComplete.text = winningText;

                _pausePageGo.SetActive(true);
                _quitButton.SetActive(true);

                StartCoroutine(ClearWinningMessage());
            }
            else {
                string currPlayerName = gameStateEvent.CurrPlayer;
                if (!string.IsNullOrEmpty(currPlayerName))
                {
                    PlayerUIController[] playerUIControllers = _playerGridGroup.GetComponentsInChildren<PlayerUIController>();
                    if (playerUIControllers != null && playerUIControllers.Length > 0)
                    {
                        foreach (var playerUIController in playerUIControllers)
                        {
                            if (playerUIController.gameObject.name.Equals(currPlayerName))
                                playerUIController.TurnMarker.enabled = true;
                            else
                                playerUIController.TurnMarker.enabled = false;
                        }
                    }
                }
            }
        }

        private void OnGameInput(object sender, IGameEvent gameEvent)
        {
            GameInputEvent gameInputEvent = (GameInputEvent)gameEvent;
            switch(gameInputEvent.State)
            {
                case GameInputEvent.States.Paused:
                        OnPause();
                    break;
            }
        }

        private IEnumerator ClearWinningMessage()
        {
            yield return new WaitForSeconds(5);
            _gameComplete.text = string.Empty;
        }

        private void OnPause()
        {
            _pausePageGo.SetActive(true);
            _PauseButton.SetActive(false);
            _quitButton.SetActive(true);
            GameManager.Instance.OnPaused();
        }

        private void OnPlayPressed()
        {
            _PauseButton.SetActive(true);
            _pausePageGo.SetActive(false);

            StartCoroutine(StartGame());
        }

        private void OnQuitPressed()
        {
            GameManager.Instance.OnQuit();
        }

        private IEnumerator StartGame()
        {
            _loadingTextGo.SetActive(true);

            if (GameManager.Instance.CurrGameState == GameManager.GameState.Practise)
            {
                EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { GameState = GameStateEvent.State.Play });
            }

            GameManager.Instance.OnGetSet();
            yield return new WaitForSeconds (3);

            if (GameManager.Instance.PrevGameState == GameManager.GameState.Practise)
            {
                PlayerUIController[] playerUIControllers = _playerGridGroup.GetComponentsInChildren<PlayerUIController>();
                if (playerUIControllers != null && playerUIControllers.Length > 0)
                {
                    foreach (var playerUIController in playerUIControllers)
                        playerUIController.NameNScore.text = playerUIController.gameObject.name + " " + 0;
                }

                GameManager.Instance.OnPlay();

                if (GameManager.Instance.NumOfTimesPlayed == 1)
                {
                    SetFirstPlayerToBreakShot();
                }
            }
            else
            {
                GameManager.Instance.OnContinue();
            }

            _loadingTextGo.SetActive(false);

            _playerGridGroup.gameObject.SetActive(true);
        }

        private void SetFirstPlayerToBreakShot()
        {
            PlayerUIController[] playerUIControllers = _playerGridGroup.GetComponentsInChildren<PlayerUIController>();
            if (playerUIControllers != null && playerUIControllers.Length > 0)
                playerUIControllers[0].TurnMarker.enabled = true;
        }

        public void CreatePlayerUI()
        {
            foreach(var player in GameManager.Instance.Players)
            {
                GameObject playerUIGo = Instantiate(_playerUITemplate);
                playerUIGo.name = player.Name;
                PlayerUIController playerUIController = playerUIGo.GetComponent<PlayerUIController>();
                playerUIController.NameNScore.text = player.Name + " " + player.Score.ToString();
                playerUIController.TurnMarker.enabled = false;
                playerUIGo.transform.SetParent(_playerGridGroup.transform);
            }
        }
    }
}
