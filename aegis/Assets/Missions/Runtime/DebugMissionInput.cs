#nullable enable
using System;
using PinkSoft.MissionSDK;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PinkSoft.Aegis.Missions
{
    /// <summary>mi-aegis 단독 테스트용 — 마우스 클릭을 InputHit으로 변환.</summary>
    public sealed class DebugMissionInput : MonoBehaviour, IMissionInput
    {
        public event Action<InputHit>? OnHit;

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return;

            var pos = mouse.position.ReadValue();
            OnHit?.Invoke(new InputHit(pos, (ulong)(Time.realtimeSinceStartup * 1_000_000)));
        }
    }
}
