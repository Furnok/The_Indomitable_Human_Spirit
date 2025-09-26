using UnityEngine;

public class S_PlayerProjectile : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    //[Header("Input")]

    //[Header("Output")]

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {

        }
    }
}