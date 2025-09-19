using UnityEngine;

public class S_TestAnimation : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] Animator AC_PlayerController;

    //[Header("Input")]

    //[Header("Output")]

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            AC_PlayerController.SetFloat("MoveSpeed", 2);
        }
        else
        {
            AC_PlayerController.SetFloat("MoveSpeed", 0);
        }
    }
}