using Sirenix.OdinInspector;
using UnityEngine;

public class S_ColliderColor : MonoBehaviour
{
    [TabGroup("Parameters")]
    [Title("Color")]
    [SerializeField] private S_ClassColor color;

    [TabGroup("References")]
    [Title("Filters")]
    [SerializeField, S_TagName] private string playerTag;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnMaterialColor rseOnMaterialColor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) rseOnMaterialColor.Call(color);
    }
}