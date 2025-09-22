using UnityEngine;

public class S_PlayerCenterPosition : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    //[Header("Input")]

    [Header("Output")]
    [SerializeField] RSE_OnPlayerAwake RSE_OnPlayerAwake;

    [Header("RSO")]
    [SerializeField] RSO_PlayerCenterPosition RSO_PlayerCenterPosition;

    private void Awake()
    {
        if (RSE_OnPlayerAwake != null)
        {
            RSE_OnPlayerAwake.Call(gameObject);
        }
    }
    private void FixedUpdate()
    {
        if (RSO_PlayerCenterPosition != null)
        {
            RSO_PlayerCenterPosition.Value = gameObject.transform.position;
        }
    }
}