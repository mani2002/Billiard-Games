using UnityEngine;
using BilliardGame.EventHandlers;
using BilliardGame.Managers;

namespace BilliardGame.Controllers
{
    public class CueBallController : MonoBehaviour
    {
        public enum CueBallType
        {
            White = 0,
            solids,
            black,
            stripes,
            first,
        }

        [SerializeField]
        float _force = 50f;

        [SerializeField]
        AudioClip btob;

        [SerializeField]
        CueBallType _ballType ;

        private CueBallActionEvent.States _currState;

        private Vector3 _initialPos;

        public bool IsPocketedInPrevTurn;

        public CueBallType BallType { get { return _ballType; } }


        private void Start()
        {
            _initialPos = transform.position;
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnDestroy()
        {
            EventManager.Unsubscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent actionEvent = (CueBallActionEvent)gameEvent;
            switch(actionEvent.State)
            {
                case CueBallActionEvent.States.Stationary:
                    {
                        _currState = CueBallActionEvent.States.Default;
                    }
                    break;
            }
        }

        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;
            switch (gameStateEvent.GameState)
            {
                case GameStateEvent.State.Play:
                    {
                        PlaceBallInInitialPos();
                    }
                    break;
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            CueController cueController = collider.gameObject.transform.parent.GetComponent<CueController>(); 
            if (cueController != null)
            {
                if (_ballType == CueBallType.White)
                {
                    EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Striked });

                    _currState = CueBallActionEvent.States.Striked;

                    float forceGatheredToHit = cueController.ForceGatheredToHit;

                    OnStriked(forceGatheredToHit);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
            {
                Debug.Log("Oncollision" + collision.gameObject.name);

                GameManager.Instance.AddToBallHitOutList(this);
            }
            if(collision.gameObject.CompareTag("cueball")&&_ballType!=CueBallType.White)
            {
                Rigidbody  m_rid=GetComponent<Rigidbody>();
                Vector3 _ballnormal = collision.contacts[0].normal;
                Rigidbody  m_rid1 =collision.gameObject.GetComponent<Rigidbody>();
                Vector3 collisionNormal = (collision.transform.position - transform.position).normalized;
                AudioSource audioSource =GetComponent<AudioSource>();
                audioSource.PlayOneShot(btob);
                float playerCollisionSpeed = Vector3.Dot (collisionNormal, m_rid.velocity);
                Vector3 m_dir = Vector3.Reflect(m_rid.velocity,_ballnormal).normalized;
                m_rid.velocity=m_dir*playerCollisionSpeed;
                Debug.Log("Oncollisionwith cue ball " + _ballType);
            }
            
        }
        
        private void FixedUpdate()
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            if ((_currState == CueBallActionEvent.States.Placing) && rigidbody.IsSleeping())
            {
                _currState = CueBallActionEvent.States.Default;
            }
            else if ((_currState == CueBallActionEvent.States.Default) && (!rigidbody.IsSleeping()))
            {
                if (GameManager.Instance.CurrGameState == GameManager.GameState.Play)
                    GameManager.Instance.NumOfBallsStriked++;
                _currState = CueBallActionEvent.States.Striked;
            }
            else if ((_currState == CueBallActionEvent.States.Striked) && (!rigidbody.IsSleeping()))
            {
                _currState = CueBallActionEvent.States.InMotion;
            }
            else if ((_currState == CueBallActionEvent.States.Striked) && (rigidbody.IsSleeping()))
            {
                _currState = CueBallActionEvent.States.InMotion;
            }
            else if ((_currState == CueBallActionEvent.States.InMotion) && rigidbody.IsSleeping())
            {
                GameManager.Instance.ReadyForNextRound();
                _currState = CueBallActionEvent.States.Stationary;
            }
            else
            {
                // else
            }
        }

        
        /// <param name="forceGathered">amount of force applied on the cue ball</param>
        private void OnStriked(float forceGathered)
        {
            if (_ballType == CueBallType.White)
            {
                GameManager.Instance.NumOfBallsStriked++;

                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
                rigidBody.AddForce(Camera.main.transform.forward * _force * forceGathered, ForceMode.Force);
            }
        }

        
        public void BallPocketed()
        {
            GameManager.Instance.AddToBallPocketedList(this);
        }

       
        public void PlaceBallInPosWhilePractise()
        {
            PlaceBallInInitialPos();

            EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
        }

        public void PlaceBallInInitialPos()
        {
            transform.position = new Vector3(_initialPos.x, _initialPos.y + 0.2f, _initialPos.z);
            IsPocketedInPrevTurn = false;
            _currState = CueBallActionEvent.States.Placing;
            GameManager.Instance.NumOfBallsStriked = 0;
        }
    }
}
