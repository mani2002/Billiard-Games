using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BilliardGame.EventHandlers;
using BilliardGame.Controllers;
using BilliardGame.UIControllers;

namespace BilliardGame.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public enum GameType
        {
            CaromBall ,
            EightBall,
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
        private GameType _gameType;
        [SerializeField]
        private GameType _gameType1;

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

        private List<CueBallController> _ballsPocketed;
        private List<CueBallController> _ballsHitOut;
        private GameState _currGameState;
        private GameState _prevGameState;
        private bool _ballsInstantiated;
        public CueBallController.CueBallType solid = CueBallController.CueBallType.solids;
        public CueBallController.CueBallType stripe = CueBallController.CueBallType.stripes;
        public int NumOfBallsStriked;

        public GameState CurrGameState { get { return _currGameState; } }
        public GameState PrevGameState { get { return _prevGameState;  } }

        public Queue<Player> Players { get { return _players;  } }

        public string Winner;

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

            int arraySize = (int)_gameType + 1;
            _ballsPocketed = new List<CueBallController>(arraySize);
            _ballsHitOut = new List<CueBallController>(arraySize);
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
                _gameType=_gameType1;
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

        private bool IsGameComplete()
        {
            CueBallController blackBall = _ballsPocketed.FirstOrDefault(b => b.BallType == CueBallController.CueBallType.black);
            if (blackBall!=null)
                return true;

            return false;
        }

        private IEnumerator OnGameComplete()
        {
            yield return new WaitForEndOfFrame();

            //int winningScore = 0;
            foreach (var player in _players)
            {
                Player currPlayer = _players.Peek();
                var ballpocketed = _ballsPocketed.Where(b => b.BallType==player.type);
                if(ballpocketed.Count()==7)
                {
                   // winningScore = player.Score;
                    Winner = currPlayer.Name;
                }
                else
                {
                    SetNewPlayerTurn();
                    currPlayer = _players.Peek();
                    Winner =currPlayer.Name;
                }
                    
            }
            //Winners = _players.Where(p => p.Score == winningScore).Select(p => p.Name).ToArray();
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
            if (currPlayer.HasStrikedBall)
            {
                CueBallController whiteBall = _ballsPocketed.FirstOrDefault(b => b.BallType == CueBallController.CueBallType.White);
                CueBallController blackBall = _ballsPocketed.FirstOrDefault(b => b.BallType == CueBallController.CueBallType.black);
                if (whiteBall != null)
                {
                    currPlayer.CalculateScore(-1);
                    _ballsPocketed.Remove(whiteBall);

                    _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);

                    whiteBall.PlaceBallInInitialPos();

                    SetNewPlayerTurn();
                }
                else if(blackBall != null)
                {
                    
                    StartCoroutine(OnGameComplete());
                }
                else
                {
                    if (_ballsPocketed.Count() > 0)
                    {
                        var ballsCurrentlyPocketed = _ballsPocketed.Where(b => b.IsPocketedInPrevTurn == false);
                        Debug.Log("Balls Currently Pocketed" + ballsCurrentlyPocketed.Count());
                        if (ballsCurrentlyPocketed.Count() > 0)
                        {   var ballsCurrentlyPocketedsolids = ballsCurrentlyPocketed.Where(b => b.BallType==solid);
                            var ballsCurrentlyPocketedstripes = ballsCurrentlyPocketed.Where(b => b.BallType==stripe);
                            if(currPlayer.type ==CueBallController.CueBallType.first&&(ballsCurrentlyPocketed.Count() == 1
                                                        ||(ballsCurrentlyPocketedsolids.Count()==ballsCurrentlyPocketed.Count())
                                                        ||(ballsCurrentlyPocketedstripes.Count()==ballsCurrentlyPocketed.Count())))
                            {
                                currPlayer.type = ballsCurrentlyPocketed.First().BallType;

                                currPlayer.CalculateScore(ballsCurrentlyPocketed.Count());
                                Player p1 = _players.Dequeue();
                                _players.Enqueue(p1);
                                p1 =_players.Peek();
                                if(currPlayer.type==solid)
                                {
                                    p1.type=stripe;
                                }
                                else
                                {
                                    p1.type = solid;
                                }
                                Player p2 = _players.Dequeue();
                                _players.Enqueue(p2);

                            }
                            else if(currPlayer.type ==CueBallController.CueBallType.first &&((ballsCurrentlyPocketedsolids.Count()!=ballsCurrentlyPocketed.Count())
                                                        ||(ballsCurrentlyPocketedstripes.Count()!=ballsCurrentlyPocketed.Count())))
                            {
                                SetNewPlayerTurn();
                            }
                            else
                            {
                                currPlayer.CalculateScore(ballsCurrentlyPocketed.Where(b => b.BallType==currPlayer.type).Count());
                                Player p1 = _players.Dequeue();
                                _players.Enqueue(p1);
                                p1 =_players.Peek();
                                p1.CalculateScore(ballsCurrentlyPocketed.Where(b => b.BallType==p1.type).Count());
                                Player p2 = _players.Dequeue();
                                _players.Enqueue(p2);
                            }
                            _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);

                        }
                        else
                        {
                            SetNewPlayerTurn();
                        }
                    }
                    else
                    {
                        SetNewPlayerTurn();
                    }
                }

                foreach (var ballHitOut in _ballsHitOut)
                    ballHitOut.PlaceBallInInitialPos();
            }
            _ballsHitOut.Clear();
            foreach (var player in _players)
            {
                player.SetPlayingState((player == _players.Peek()));
            }
            if (IsGameComplete())
                StartCoroutine(OnGameComplete());
            else
                EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
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
            _ballsHitOut.Clear();
            _ballsPocketed.Clear();

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
           
        }

        public void AddToBallPocketedList(CueBallController ball)
        {
            if (!_ballsPocketed.Contains(ball))
                _ballsPocketed.Add(ball);
        }

        public void AddToBallHitOutList(CueBallController ball)
        {
            if (!_ballsHitOut.Contains(ball) && !_ballsPocketed.Contains(ball))
                _ballsHitOut.Add(ball);
        }
    }
}
