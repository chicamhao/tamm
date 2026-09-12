/*
Disables PlayerInput auto control-scheme switching on mobile builds; auto-switch scans the
device for connected devices, which is costly where controls are fixed (screen, tilt).
*/

using UnityEngine;

namespace Game.Mobile.Utilities
{
    public sealed class MobileDisableAutoSwitchControls : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM && (UNITY_IOS || UNITY_ANDROID)

    [Header("Target")]
    public PlayerInput playerInput;

    void Start()
    {
        DisableAutoSwitchControls();
    }

    void DisableAutoSwitchControls()
    {
        playerInput.neverAutoSwitchControlSchemes = true;
    }

#endif
    }
}