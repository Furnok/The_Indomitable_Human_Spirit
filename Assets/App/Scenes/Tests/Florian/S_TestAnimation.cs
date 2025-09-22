using UnityEngine;

public class S_TestAnimation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] string ParameterName;

    //[Header("References")]

    //[Header("Input")]

    [Header("Output")]
    [SerializeField] RSE_OnAnimationBoolValueChange RSE_OnAnimationBoolValueChange;

    private void Update()
    {
        if (Input.GetKey(KeyCode.F))
        {
            RSE_OnAnimationBoolValueChange.Call(ParameterName,true);
        }
        else
        {
            RSE_OnAnimationBoolValueChange.Call(ParameterName, false);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            RSE_OnAnimationBoolValueChange.Call("isDodging", true);
        }
        else
        {
            RSE_OnAnimationBoolValueChange.Call("isDodging", false);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RSE_OnAnimationBoolValueChange.Call("isHit", true);
        }
        else
        {
            RSE_OnAnimationBoolValueChange.Call("isHit", false);
        }
    }
}