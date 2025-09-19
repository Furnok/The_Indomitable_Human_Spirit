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
        if (Input.GetKey(KeyCode.F))
        {
            AC_PlayerController.SetBool("Parry", true);
        }
        else
        {
            AC_PlayerController.SetBool("Parry", false);
        }
    }
}