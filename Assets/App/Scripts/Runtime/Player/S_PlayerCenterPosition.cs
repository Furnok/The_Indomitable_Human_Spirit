using UnityEngine;

public class S_PlayerCenterPosition : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    //[Header("Input")]

    //[Header("Output")]

    [Header("RSO")]
    [SerializeField] RSO_PlayerCenterPosition RSO_PlayerCenterPosition;

    private void FixedUpdate()
    {
        if (RSO_PlayerCenterPosition != null)
        {
            RSO_PlayerCenterPosition.Value = gameObject.transform.position;
        }
    }
}