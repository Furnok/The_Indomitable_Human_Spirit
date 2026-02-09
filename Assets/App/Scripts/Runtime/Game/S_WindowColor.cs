using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class S_WindowColor : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Windows")]
    [SerializeField] private List<MeshRenderer> meshRenderers;

    [TabGroup("References")]
    [Title("Light")]
    [SerializeField] private List<GameObject> lights;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnMaterialColor rseOnMaterialColor;

    private List<Material> materialInstance = new();

    private void OnEnable()
    {
        rseOnMaterialColor.action += ChangeMaterial;

        materialInstance.Clear();

        foreach (Renderer renderer in meshRenderers)
        {
            materialInstance.Add(renderer.material);
        }
    }

    private void OnDisable()
    {
        rseOnMaterialColor.action -= ChangeMaterial;
    }

    private void ChangeMaterial(S_ClassColor color)
    {
        foreach (Material mat in materialInstance)
        {
            HDMaterial.SetEmissiveColor(mat, color.color);
            HDMaterial.SetEmissiveIntensity(mat, color.intensity, EmissiveIntensityUnit.EV100);
        }

        foreach (GameObject light in lights)
        {
            if (color.intensity != 0) light.SetActive(true);
            else light.SetActive(false);
        }
    }
}