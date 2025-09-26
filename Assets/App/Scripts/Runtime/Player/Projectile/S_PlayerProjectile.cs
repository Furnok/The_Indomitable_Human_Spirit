using UnityEngine;

public class S_PlayerProjectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {

        }
    }
}