using UnityEngine;

public class S_PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator playerAnimator;

    [Header("Input")]
    [SerializeField] RSE_OnAnimationBoolValueChange RSE_OnAnimationBoolValueChange;
    [SerializeField] RSE_OnAnimationFloatValueChange RSE_OnAnimationFloatValueChange;
    [SerializeField] RSE_OnAnimationIntValueChange RSE_OnAnimationIntValueChange;

    private void OnEnable()
    {
        RSE_OnAnimationFloatValueChange.action += AnimatorSetFloatValue;
        RSE_OnAnimationIntValueChange.action += AnimatorSetIntValue;
        RSE_OnAnimationBoolValueChange.action += AnimatorSetBoolValue;
    }

    private void OnDisable()
    {
        RSE_OnAnimationFloatValueChange.action -= AnimatorSetFloatValue;
        RSE_OnAnimationIntValueChange.action -= AnimatorSetIntValue;
        RSE_OnAnimationBoolValueChange.action -= AnimatorSetBoolValue;
    }

    void AnimatorSetBoolValue(string parameterName, bool value)
    {
        playerAnimator.SetBool(parameterName, value);
    }

    void AnimatorSetIntValue(string parameterName, int value)
    {
        playerAnimator.SetInteger(parameterName, value);
    }

    void AnimatorSetFloatValue(string parameterName, float value)
    {
        playerAnimator.SetFloat(parameterName, value);
    }
}