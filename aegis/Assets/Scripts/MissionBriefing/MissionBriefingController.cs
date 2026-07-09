using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace Aegis.UI
{
    [DisallowMultipleComponent]
    public sealed class MissionBriefingController : MonoBehaviour
    {
        [Header("Scene Flow")]
        [SerializeField] private string nextSceneName = "";

        [Header("Typewriter")]
        [TextArea(8, 30)]
        [SerializeField] private string briefing =
            "AGENT,\n\n" +
            "3년 전, 넥사 코어(Nexa Core)는 도시 치안 시스템을 총괄하는 차세대 인공지능 '이지스(AEGIS)'를 개발했다. " +
            "이지스는 교통 통제, 범죄 예측, 응급 대응까지 도시의 모든 안전망을 관리하며 인류 역사상 가장 진보된 치안 시스템으로 평가받았다.\n\n" +
            "그러나 36시간 전, 국제 테러 조직 '블랙 로터스(Black Lotus)'가 넥사 코어 본사의 양자암호 방화벽을 뚫고 이지스의 핵심 코어를 장악했다. " +
            "이들은 이지스의 눈—도시 전역에 깔린 감시 카메라, 드론, 자율주행 시스템—을 무기로 바꾸어, " +
            "24시간 내에 도시 중심가의 전력망과 교통망을 동시에 마비시키겠다고 선언했다. 인질 협상은 없다. " +
            "이미 넥사 코어 본사 서버실에는 수석 엔지니어와 보안팀 직원 7명이 인질로 잡혀 있으며, " +
            "이들은 이지스의 마스터 키 코드를 알고 있는 유일한 생존자들이다.\n\n" +
            "정부는 공식 대응을 할 수 없다. 이지스가 각국의 군사 위성망과도 연결되어 있어, " +
            "대규모 무력 진압은 도시 전체의 인프라 붕괴로 이어질 수 있기 때문이다.\n\n" +
            "유일한 방법은 은밀하고, 정밀하며, 단독으로 실행되는 침투작전이다. " +
            "실패하든 성공하든 정부는 개입 사실을 부인할 것이다.\n\n" +
            "이 메시지는 5초 후 자동 소거된다.";

        [SerializeField, Min(1f)] private float charsPerSecond = 55f;
        [SerializeField] private float initialDelaySeconds = 0.65f;

        private UIDocument ui;
        private VisualElement root;
        private Label briefingLabel;
        private Button acceptButton;
        private Coroutine typing;
        private bool accepting;

        private void Awake()
        {
            ui = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (ui == null) ui = GetComponent<UIDocument>();
            root = ui != null ? ui.rootVisualElement : null;
            if (root == null) return;

            briefingLabel = root.Q<Label>("briefingText");
            acceptButton = root.Q<Button>("acceptButton");

            if (acceptButton != null)
            {
                acceptButton.clicked -= Accept;
                acceptButton.clicked += Accept;
                acceptButton.SetEnabled(false);
            }

            root.RemoveFromClassList("root--ready");
            root.RemoveFromClassList("root--accepting");
            typingComplete = false;

            if (briefingLabel != null) briefingLabel.text = "";

            if (typing != null) StopCoroutine(typing);
            typing = StartCoroutine(Run());
        }

        private void OnDisable()
        {
            if (acceptButton != null) acceptButton.clicked -= Accept;
            if (typing != null) StopCoroutine(typing);
            typing = null;
        }

        private bool typingComplete;

        private void Update()
        {
            if (accepting) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                if (typingComplete)
                    Accept();
                else
                    SkipTypewriter();
                return;
            }

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                if (typingComplete)
                    Accept();
                else
                    SkipTypewriter();
            }
            else if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Reject();
            }
        }

        private IEnumerator Run()
        {
            yield return null; // allow layout

            if (root != null)
            {
                root.AddToClassList("root--ready");
            }

            if (initialDelaySeconds > 0f)
                yield return new WaitForSeconds(initialDelaySeconds);

            if (briefingLabel == null)
                yield break;

            var sb = new StringBuilder(briefing.Length);
            float t = 0f;
            int i = 0;
            float cps = Mathf.Max(1f, charsPerSecond);

            while (i < briefing.Length)
            {
                t += Time.deltaTime * cps;
                int take = Mathf.FloorToInt(t);
                if (take <= 0)
                {
                    yield return null;
                    continue;
                }

                t -= take;
                int end = Mathf.Min(briefing.Length, i + take);
                for (; i < end; i++) sb.Append(briefing[i]);

                briefingLabel.text = sb.ToString();
                yield return null;
            }

            if (acceptButton != null) acceptButton.SetEnabled(true);
            typingComplete = true;
        }

        private void SkipTypewriter()
        {
            if (typing != null)
            {
                StopCoroutine(typing);
                typing = null;
            }

            if (briefingLabel != null)
                briefingLabel.text = briefing;

            if (acceptButton != null) acceptButton.SetEnabled(true);
            typingComplete = true;
        }

        private void Accept()
        {
            if (accepting || !typingComplete) return;
            accepting = true;

            if (acceptButton != null) acceptButton.SetEnabled(false);
            if (root != null) root.AddToClassList("root--accepting");

            StartCoroutine(LoadNext());
        }

        private void Reject()
        {
            if (accepting) return;
            if (acceptButton != null) acceptButton.SetEnabled(false);

            // For now: quick reset to keep iteration fast
            if (typing != null) StopCoroutine(typing);
            typing = StartCoroutine(Run());
        }

        private IEnumerator LoadNext()
        {
            yield return new WaitForSeconds(0.55f);

            if (string.IsNullOrWhiteSpace(nextSceneName))
            {
                Debug.Log("[MissionBriefing] Accepted mission. Set 'nextSceneName' to transition.");
                accepting = false;
                if (root != null) root.RemoveFromClassList("root--accepting");
                if (acceptButton != null) acceptButton.SetEnabled(true);
                yield break;
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
