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
    private bool inFight = false;
    private bool inTemp = false;

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
            inTemp = false;
        }

        _weaponHand.SetActive(true);
        _weaponBack.SetActive(false);

        inFight = true;
    }

    private void HideWeaponArm()
    {
        if (!inTemp)
        {
            if (displayWeaponArmTempCoroutine != null)
            {
                StopCoroutine(displayWeaponArmTempCoroutine);
                displayWeaponArmTempCoroutine = null;
                inTemp = false;
            }

            _weaponHand.SetActive(false);
            _weaponBack.SetActive(true);
        }

        inFight = false;
    }

    private void DisplayWeaponArmTemp()
    {
        if (!inFight)
        {
            if (displayWeaponArmTempCoroutine != null)
            {
                StopCoroutine(displayWeaponArmTempCoroutine);
                displayWeaponArmTempCoroutine = null;
                inTemp = false;
            }

            _weaponHand.SetActive(true);
            _weaponBack.SetActive(false);
        }

        inTemp = true;
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
        if (!inFight)
        {
            _weaponHand.SetActive(false);
            _weaponBack.SetActive(true);
        }

        inTemp = false;
    }
}