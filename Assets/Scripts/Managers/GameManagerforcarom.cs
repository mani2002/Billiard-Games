using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BilliardGame.EventHandlers;
using BilliardGame.Controllers;
using BilliardGame.UIControllers;

namespace BilliardGame.Managers
{
   
    public class GameManagerForCarom : Singleton<GameManagerForCarom>
    {
        public enum GameType
        {
            CaromBall,
        }

        public enum GameState
        {
            Practise = 1,
            GetSet,
            Play,
            Pause,
            Complete
        }
        [SerializeField]
        private string[] _playerNames;

        [SerializeField]
        private GameType _gameType= GameType.CaromBall;

        [SerializeField]
        private Transform  _rackTransform;

        [SerializeField]
        private Transform  _rackTransform1;

        [SerializeField]
        private CueBallController _cueBall;

        [SerializeField]
        private CueBallController _cueBall1;

        [SerializeField]
        private GameUIScreen _gameUIScreen;

        [SerializeField]
        private GameUIScreen _gameUIScreen1;

        [SerializeField]
        private GameObject _FPS;

        [SerializeField]
        private GameObject _UI;

        [SerializeField]
        private GameObject _UI1;
        [SerializeField]
        private GameObject _topcamera;
        [SerializeField]
        private GameObject _topcamera1;
        [SerializeField]
        private GameObject _camera;
        [SerializeField]
        private GameObject _camera1;
        [SerializeField]
        private GameObject _canvas;

        private Queue<Player> _players = new Queue<Player>();

        
        private GameState _currGameState;
        private GameState _prevGameState;
        private bool _ballsInstantiated;
        public int NumOfBallsStriked;

        public GameState CurrGameState { get { return _currGameState; } }
        public GameState PrevGameState { get { return _prevGameState;  } }

        public Queue<Player> Players { get { return _players;  } }

        public string[] Winners;

        public int NumOfTimesPlayed { private set; get; }

        protected override void Start()
        {
            base.Start();

            ChangeGameState(GameState.Practise);
            NumOfBallsStriked = 0;

            if (_playerNames != null)
            {
                foreach (var playerName in _playerNames)
                {
                    var player = new Player(playerName);

                    _players.Enqueue(player);
                }
            }

            _gameUIScreen.CreatePlayerUI();
            _gameUIScreen1.CreatePlayerUI();

        }
        protected override void Update()
        {
            if(Input.GetKey(KeyCode.P)&&(Input.GetKeyDown(KeyCode.Alpha1)||Input.GetKeyDown(KeyCode.Keypad1)))
            {
                _FPS.SetActive(false);
                _UI.SetActive(true);
                _topcamera.SetActive(true);
                _topcamera1.SetActive(false);
                _camera.SetActive(true);
                _camera1.SetActive(false);
                _canvas.SetActive(false);
            }
            if(Input.GetKey(KeyCode.P)&&(Input.GetKeyDown(KeyCode.Alpha2)||Input.GetKeyDown(KeyCode.Keypad2)))
            {
                _FPS.SetActive(false);
                _UI1.SetActive(true);
                _topcamera1.SetActive(true);
                _topcamera.SetActive(false);
                _camera1.SetActive(true);
                _camera.SetActive(false);
                _canvas.SetActive(false);
                _rackTransform=_rackTransform1;
                _cueBall=_cueBall1;
                _gameUIScreen=_gameUIScreen1;
                
                
            }
            


        }
        private void PlaceBallBasedOnGameType()
        {
            string rackString = "Rack";
            Instantiate((Resources.Load(_gameType.ToString() + rackString, typeof(GameObject)) as GameObject), _rackTransform.position, _rackTransform.rotation);
           
        }
        /*
        private bool IsGameComplete()
        {
            if (blackBall!=null)
                return true;

            return false;
        }*/

        private IEnumerator OnGameComplete()
        {
            yield return new WaitForEndOfFrame();

            int winningScore = 0;

            foreach (var player in _players)
            {
                
                if (player.Score >= winningScore)
                {
                    winningScore = player.Score;
                }
            }

            Winners = _players.Where(p => p.Score == winningScore).Select(p => p.Name).ToArray();
            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { GameState = GameStateEvent.State.Complete });
        }

        private void SetNewPlayerTurn()
        {
            Player player = _players.Dequeue();
            _players.Enqueue(player);
            Player newPlayer = _players.Peek();
            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { CurrPlayer = newPlayer.Name });
        }

        private void CalculateThePointAndNextTurn()
        {
            
            Player currPlayer = _players.Peek();

        }

        public void ChangeGameState(GameState newGameState)
        {
            if(newGameState != _currGameState)
            {
                _prevGameState = _currGameState;
                _currGameState = newGameState;
            }
        }

        public void OnGetSet()
        {
            ChangeGameState(GameState.GetSet);
        }

        public void OnPlay()
        {
            

            NumOfBallsStriked = 0;

            NumOfTimesPlayed++;

            foreach (var player in _players)
                player.ResetScore();

            ChangeGameState(GameState.Play);

            _cueBall.PlaceBallInInitialPos();

            if (!_ballsInstantiated)
            {
                PlaceBallBasedOnGameType();

                _ballsInstantiated = true;
            }
        }

        public void OnPaused()
        {
            ChangeGameState(GameState.Pause);
        }

        public void OnContinue()
        {
            ChangeGameState(GameState.Play);
        }
        public void OnQuit()
        {
            ChangeGameState(GameState.Practise);
            _FPS.SetActive(true);
            _UI.SetActive(false);
            _topcamera.SetActive(false);
            _topcamera1.SetActive(false);
            _camera.SetActive(false);
            _camera1.SetActive(false);
            _canvas.SetActive(true);
            _UI1.SetActive(false);
        }


        public void ReadyForNextRound()
        {
            if (CurrGameState == GameState.Practise)
            {
                _cueBall.PlaceBallInPosWhilePractise();
            }
            else if(CurrGameState == GameState.Play || CurrGameState == GameState.Pause)
            {
                NumOfBallsStriked--;

                if (NumOfBallsStriked == 0)
                    CalculateThePointAndNextTurn();
            }
            else
            {
                // else
            }
        }

    }
}
