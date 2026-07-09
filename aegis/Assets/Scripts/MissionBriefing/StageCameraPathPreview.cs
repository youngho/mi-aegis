using UnityEngine;
using UnityEngine.InputSystem;

namespace Aegis.UI
{
    [DisallowMultipleComponent]
    public sealed class StageCameraPathPreview : MonoBehaviour
    {
        [SerializeField] private Transform cameraPathRoot;
        [SerializeField, Min(0.1f)] private float moveSpeed = 6f;
        [SerializeField, Min(0.01f)] private float rotateSpeed = 180f;
        [SerializeField, Min(0f)] private float arriveDistance = 0.05f;
        [SerializeField] private bool loop;

        private Transform[] points;
        private int idx;
        private bool playing;

        private void Awake()
        {
            if (cameraPathRoot == null)
            {
                var go = GameObject.Find("Stage1_Lobby/CameraPath_Stage1")
                         ?? GameObject.Find("AegisMission/Stages/Stage1_Lobby/CameraPath_Stage1");
                cameraPathRoot = go != null ? go.transform : null;
            }

            CachePoints();
        }

        private void CachePoints()
        {
            if (cameraPathRoot == null)
            {
                points = null;
                return;
            }

            int childCount = cameraPathRoot.childCount;
            points = new Transform[childCount];
            for (int i = 0; i < childCount; i++) points[i] = cameraPathRoot.GetChild(i);
        }

        private void Update()
        {
            // P: play/pause, 0: restart
            if (Keyboard.current != null)
            {
                if (Keyboard.current.pKey.wasPressedThisFrame) playing = !playing;
                if (Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame)
                {
                    idx = 0;
                    if (points != null && points.Length > 0)
                    {
                        transform.position = points[0].position;
                        transform.rotation = points[0].rotation;
                    }
                }
            }

            if (!playing) return;
            if (points == null || points.Length == 0) return;
            if (idx >= points.Length) return;

            var target = points[idx];
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) <= arriveDistance)
            {
                idx++;
                if (idx >= points.Length)
                {
                    if (loop) idx = 0;
                    else playing = false;
                }
            }
        }
    }
}
