using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class S_HitEffects : MonoBehaviour
{
	[TabGroup("Settings")]
	[Title("Test")]
	[SerializeField] private float min;

    [TabGroup("Settings")]
    [SerializeField] private float max;

    [TabGroup("References")]
    [Title("Material")]
    [SerializeField] private Material screenDamageMat;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerGettingHit rseOnPlayerGettingHit;

    private Material materialInstance = null;
    private Coroutine screenDamageTask = null;

    private void OnEnable()
	{
        rseOnPlayerGettingHit.action += Hit;

		materialInstance = screenDamageMat;
    }

	private void OnDisable()
	{
        rseOnPlayerGettingHit.action -= Hit;

        materialInstance.SetFloat("_Vignette_radius", 1);
    }

    private void Hit()
    {
        ScreenDamageEffect(Random.Range(min, max));
    }

    private void ScreenDamageEffect(float intensity) 
	{
		if (screenDamageTask != null)
		{
			StopCoroutine(screenDamageTask);
			screenDamageTask = null;
        }

		screenDamageTask = StartCoroutine(screenDamage(intensity));
	}
	private IEnumerator screenDamage(float intensity)
	{
		var targetRadius = Remap(intensity, 0, 1, 0.5f, 0f);
		var curRadius = 1f;

		for(float t = 0; curRadius != targetRadius; t += Time.deltaTime)
		{
			curRadius = Mathf.Clamp(Mathf.Lerp(1, targetRadius, t), 1, targetRadius);
            materialInstance.SetFloat("_Vignette_radius", curRadius);
			yield return null;
		}

		for(float t = 0; curRadius < 1; t += Time.deltaTime)
		{
			curRadius = Mathf.Lerp(targetRadius, 1, t);
            materialInstance.SetFloat("_Vignette_radius", curRadius);
			yield return null;
		}	
	}

	private float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
	{
		return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
	}
}