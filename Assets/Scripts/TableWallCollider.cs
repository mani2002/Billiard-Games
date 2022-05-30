using UnityEngine;
using BilliardGame.Controllers;
using BilliardGame.Managers;

namespace BilliardGame
{
    public class TableWallCollider : MonoBehaviour
    {
        private void OnTriggerStay(Collider collider)
        {
            CueBallController cueBallController = collider.gameObject.GetComponent<CueBallController>();
            if (cueBallController != null && cueBallController.GetComponent<Rigidbody>().IsSleeping())
            {
                GameManager.Instance.AddToBallHitOutList(cueBallController);
                //AudioSource audioSource = GetComponent<AudioSource>();
                //audioSource.Play();
            }
             
        }
       
       
    }
}
