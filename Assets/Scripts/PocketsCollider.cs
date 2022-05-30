using UnityEngine;
using BilliardGame.Controllers;

namespace BilliardGame
{
    class PocketsCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider collider)
        {
            AudioSource audioSource = GetComponent<AudioSource>();;
            audioSource.Play();
            CueBallController cueBall = collider.gameObject.GetComponent<CueBallController>();
            if (cueBall != null)
                cueBall.BallPocketed();
        }
    }
}
