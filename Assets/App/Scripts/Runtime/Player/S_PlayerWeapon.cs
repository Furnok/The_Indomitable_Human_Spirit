using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class S_PlayerWeapon : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Weapons")]
    [SerializeField] private GameObject _weaponHand;

    [TabGroup("References")]
    [SerializeField] private GameObject _weaponBack;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDisplayWeaponArm rseOnDisplayWeaponArm;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDisplayWeaponArmTemp rseOnDisplayWeaponArmTemp;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnHideWeaponArm rseOnHideWeaponArm;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnHideWeaponArmTemp rseOnHideWeaponArmTemp;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerIsTargeting rsoPlayerIsTargeting;

    private Coroutine displayWeaponArmTempCoroutine = null;

    private void Awake()
    {
        _weaponHand.SetActive(false);
        _weaponBack.SetActive(true);
    }

    private void OnEnable()
    {
        rseOnDisplayWeaponArm.action += DisplayWeaponArm;
        rseOnDisplayWeaponArmTemp.action += DisplayWeaponArmTemp;
        rseOnHideWeaponArm.action += HideWeaponArm;
        rseOnHideWeaponArmTemp.action += HideWeaponArmTemp;
    }

    private void OnDisable()
    {
        rseOnDisplayWeaponArm.action -= DisplayWeaponArm;
        rseOnDisplayWeaponArmTemp.action -= DisplayWeaponArmTemp;
        rseOnHideWeaponArm.action -= HideWeaponArm;
        rseOnHideWeaponArmTemp.action -= HideWeaponArmTemp;
    }

    private void DisplayWeaponArm()
    {
        if (displayWeaponArmTempCoroutine != null)
        {
            StopCoroutine(displayWeaponArmTempCoroutine);
            displayWeaponArmTempCoroutine = null;
        }

        _weaponHand.SetActive(true);
        _weaponBack.SetActive(false);
    }

    private void HideWeaponArm()
    {
        if (displayWeaponArmTempCoroutine != null)
        {
            StopCoroutine(displayWeaponArmTempCoroutine);
            displayWeaponArmTempCoroutine = null;
        }

        _weaponHand.SetActive(false);
        _weaponBack.SetActive(true);
    }

    private void DisplayWeaponArmTemp()
    {
        if (!rsoPlayerIsTargeting.Value)
        {
            if (displayWeaponArmTempCoroutine != null)
            {
                StopCoroutine(displayWeaponArmTempCoroutine);
                displayWeaponArmTempCoroutine = null;
            }

            _weaponHand.SetActive(true);
            _weaponBack.SetActive(false);
        }
    }

    private void HideWeaponArmTemp(float time)
    {
        if (displayWeaponArmTempCoroutine != null)
        {
            StopCoroutine(displayWeaponArmTempCoroutine);
            displayWeaponArmTempCoroutine = null;
        }

        displayWeaponArmTempCoroutine = StartCoroutine(DisplayWeaponArmTempCoroutine(time));
    }

    private IEnumerator DisplayWeaponArmTempCoroutine(float time)
    {
        yield return new WaitForSeconds(time);

        HideWeaponArmTime();
    }

    private void HideWeaponArmTime()
    {
        if (!rsoPlayerIsTargeting.Value)
        {
            _weaponHand.SetActive(false);
            _weaponBack.SetActive(true);
        }
    }
}