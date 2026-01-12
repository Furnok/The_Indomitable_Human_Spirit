using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class S_RumbleManager : MonoBehaviour
{
    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnRumbleRequested _onRumbleRequested;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnRumbleStopChannel _onRumbleStopChannel;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnRumbleStop _onRumbleStop;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_SettingsSaved _rsoSettingsSaved;

    private class ActiveRumble
    {
        public S_StructRumbleData data;
        public float elapsed;
    }

    private readonly List<ActiveRumble> _activeRumbles = new();

    private void OnEnable()
    {
        _onRumbleRequested.action += OnRumbleRequested;
        _onRumbleStopChannel.action += OnRumbleStopChannel;
        _onRumbleStop.action += StopAllRumble;
    }

    private void OnDisable()
    {
        _onRumbleRequested.action -= OnRumbleRequested;
        _onRumbleStopChannel.action -= OnRumbleStopChannel;
        _onRumbleStop.action -= StopAllRumble;

        StopAllRumble();
    }

    private void OnDestroy()
    {
        StopAllRumble();
    }

    private void OnRumbleRequested(S_StructRumbleData rumbleData)
    {
        if (Gamepad.current == null || !_rsoSettingsSaved.Value.controllerRumble) return;

        _activeRumbles.Add(new ActiveRumble
        {
            data = rumbleData,
            elapsed = 0f
        });
    }

    private void Update()
    {
        if (!_rsoSettingsSaved.Value.controllerRumble) return;

        if (_activeRumbles.Count == 0)
        {
            ApplyMotorSpeeds(0, 0);
            return;
        }

        float dt;
        float low = 0f;
        float high = 0f;

        for (int i = _activeRumbles.Count - 1; i >= 0; i--)
        {
            var r = _activeRumbles[i];

            dt = r.data.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            r.elapsed += dt;

            float t01 = Mathf.Clamp01(r.elapsed / Mathf.Max(r.data.Duration, 0.0001f));
            float env = r.data.CurveIntensityInTime != null ? r.data.CurveIntensityInTime.Evaluate(t01) : 1f;

            low += r.data.LowFrequency * env;
            high += r.data.HighFrequency * env;

            if (r.elapsed >= r.data.Duration)
            {
                _activeRumbles.RemoveAt(i);
            }
        }

        low = Mathf.Clamp01(low);
        high = Mathf.Clamp01(high);

        ApplyMotorSpeeds(low, high);
    }

    private void ApplyMotorSpeeds(float low, float high)
    {
        var pad = Gamepad.current;
        if (pad == null) return;

        pad.SetMotorSpeeds(low, high);
    }

    private void StopAllRumble()
    {
        _activeRumbles.Clear();

        var pad = Gamepad.current;
        if (pad != null) pad.SetMotorSpeeds(0f, 0f);
    }

    private void OnRumbleStopChannel(S_EnumRumbleChannel channel)
    {
        if (Gamepad.current == null || !_rsoSettingsSaved.Value.controllerRumble) return;

        for (int i = _activeRumbles.Count - 1; i >= 0; i--)
        {
            if (_activeRumbles[i].data.Channel == channel)
            {
                _activeRumbles.RemoveAt(i);
                ApplyMotorSpeeds(0f, 0f);
            }
        }

        if (_activeRumbles.Count == 0) ApplyMotorSpeeds(0f, 0f);
    }
}