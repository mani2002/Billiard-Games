using UnityEngine;
using BilliardGame.Controllers;
using BilliardGame.Managers;
namespace BilliardGame
{

    public class BorderCollider : MonoBehaviour
    {
        Vector3 normal;
        
        void start()
        {
            normal = gameObject.GetComponent<Transform>().up;
        }
        private void OnCollisionrEnter(Collision collider)
        {
            if (collider.gameObject.CompareTag("cueball"))
            {
                Rigidbody rd = collider.gameObject.GetComponent<Rigidbody>();
                //vector3 normal = gameObject.normal;
                //rd.velocity = Vector3.Reflect(collider.gameObject.GetComponent<Transform>().position, normal);
                rd.velocity = Vector3.Reflect(rd.velocity, normal);
            }
            
                /*Rigidbody  m_rid=collider.gameObject.GetComponent<Rigidbody>();
                Vector3 _ballnormal = collider.contacts[0].normal;
                Rigidbody  m_rid1 = GetComponent<Rigidbody>();
                Vector3 collisionNormal = (collider.transform.position - transform.position).normalized;
                float playerCollisionSpeed = Vector3.Dot (collisionNormal, m_rid.velocity);
                //float v = m_rid1.Speed;
                Vector3 m_dir = Vector3.Reflect(m_rid.velocity,_ballnormal).normalized;
                m_rid.velocity=m_dir*playerCollisionSpeed;
                Debug.Log("Oncollisionwith cue ball ");*/
        }
    }
}
