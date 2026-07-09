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
            var keyboard = Keyboard.current;

            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                EmitHit(mouse.position.ReadValue());
                return;
            }

            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                EmitHit(GetScreenCenter());
        }

        static Vector2 GetScreenCenter() => new(Screen.width * 0.5f, Screen.height * 0.5f);

        void EmitHit(Vector2 screenPosition) =>
            OnHit?.Invoke(new InputHit(screenPosition, (ulong)(Time.realtimeSinceStartup * 1_000_000)));
    }
}
