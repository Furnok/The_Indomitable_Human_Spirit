using UnityEngine;

public class S_PlayerMeditation : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    [Header("Input")]
    [SerializeField] RSE_OnPlayerMeditation _onPlayerMeditation;
    [SerializeField] RSE_OnPlayerMeditationCancel _onPlayerMeditationCancel;

    //[Header("Output")]

    private void OnEnable()
    {
        _onPlayerMeditation.action += Meditation;
        _onPlayerMeditationCancel.action += CancelMeditation;
    }

    private void OnDisable()
    {
        _onPlayerMeditation.action -= Meditation;
        _onPlayerMeditationCancel.action -= CancelMeditation;
    }

    void Meditation()
    {
        Debug.Log("Player Meditation");
    }

    void CancelMeditation()
    {
        Debug.Log("Player Cancel Meditation");
    }
}