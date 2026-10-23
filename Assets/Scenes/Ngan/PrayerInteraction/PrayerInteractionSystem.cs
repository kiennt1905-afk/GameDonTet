using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;   // New Input System
using StarterAssets;

namespace BuddhistTemple
{
    /// <summary>
    /// PrayerInteractionSystem — Buddhist prayer sequence with camera-driven animation.
    ///
    /// Sequence khi nhấn E:
    ///   1. Khóa input, xoay người về phía tượng Phật (0.6s)
    ///   2. Camera ngẩng lên nhìn tượng Phật (1s)  → giữ 3s
    ///   3. Dập đầu 3 lần (camera cúi xuống → ngẩng lên, mỗi lần ~1.4s)
    ///   4. Camera trả về vị trí ban đầu (0.8s)
    ///   5. Mở lại input, ẩn UI
    /// </summary>
    [AddComponentMenu("Buddhist Temple/Prayer Interaction System")]
    public class PrayerInteractionSystem : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────────────
        //  Inspector — References
        // ─────────────────────────────────────────────────────────────────────

        [Header("References — Scene Objects")]
        [Tooltip("Transform của tượng Phật. Player sẽ xoay về phía này.")]
        public Transform buddhaStatue;

        [Tooltip("GameObject PlayerCapsule chứa FirstPersonController.")]
        public GameObject playerObject;

        [Tooltip("AudioSource phát tiếng chuông chùa.")]
        public AudioSource templeBellAudioSource;

        [Tooltip("AudioClip tiếng chuông chùa.")]
        public AudioClip templeBellClip;

        [Tooltip("AudioSource phát nhạc nền.")]
        public AudioSource backgroundAudioSource;

        [Header("References — UI")]
        [Tooltip("CanvasGroup của panel 'Press E to Pray'.")]
        public CanvasGroup prayPromptCanvasGroup;

        [Tooltip("CanvasGroup của panel trạng thái đang cầu nguyện.")]
        public CanvasGroup prayingStatusCanvasGroup;

        // ─────────────────────────────────────────────────────────────────────
        //  Inspector — Prayer Animation Settings
        // ─────────────────────────────────────────────────────────────────────

        [Header("Prayer Animation")]
        [Tooltip("Thời gian xoay người về phía tượng (giây).")]
        public float rotationDuration = 0.6f;

        [Tooltip("Góc ngẩng đầu nhìn lên tượng Phật (độ, dương = nhìn lên).")]
        [Range(0f, 60f)]
        public float lookUpAngle = 25f;

        [Tooltip("Thời gian camera ngẩng lên (giây).")]
        public float lookUpDuration = 1.0f;

        [Tooltip("Thời gian giữ nhìn tượng Phật (giây).")]
        public float lookUpHoldDuration = 3.0f;

        [Tooltip("Số lần dập đầu.")]
        [Range(1, 10)]
        public int bowCount = 3;

        [Tooltip("Góc cúi đầu khi dập đầu (độ, dương = cúi xuống).")]
        [Range(10f, 89f)]
        public float bowAngle = 55f;

        [Tooltip("Thời gian cúi xuống (giây).")]
        public float bowDownDuration = 0.55f;

        [Tooltip("Thời gian ngẩng lên sau mỗi lần dập đầu (giây).")]
        public float bowUpDuration = 0.7f;

        [Tooltip("Thời gian dừng ở điểm cúi thấp nhất (giây).")]
        public float bowHoldAtBottom = 0.15f;

        [Tooltip("Thời gian camera về vị trí ban đầu (giây).")]
        public float returnDuration = 0.8f;

        [Tooltip("Tốc độ fade UI.")]
        public float uiFadeSpeed = 5.0f;

        // ─────────────────────────────────────────────────────────────────────
        //  Private State
        // ─────────────────────────────────────────────────────────────────────

        private FirstPersonController                _fpsController;
        private StarterAssetsInputs                  _starterInputs;
        private UnityEngine.InputSystem.PlayerInput  _playerInput;

        // Camera root (PlayerCameraRoot) — controlled trực tiếp khi FPC bị tắt
        private Transform _cameraRoot;

        // Lưu pitch gốc của FPC để restore sau khi xong
        private float _originalCinemachinePitch = 0f;

        private bool _playerInZone  = false;
        private bool _isPraying     = false;
        private bool _promptVisible = false;

        // ─────────────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (playerObject == null)
                playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                _fpsController = playerObject.GetComponent<FirstPersonController>();
                _starterInputs = playerObject.GetComponent<StarterAssetsInputs>();
                _playerInput   = playerObject.GetComponent<UnityEngine.InputSystem.PlayerInput>();

                // PlayerCameraRoot là child của PlayerCapsule
                _cameraRoot = playerObject.transform.Find("PlayerCameraRoot");
            }

            if (prayPromptCanvasGroup  != null) prayPromptCanvasGroup.alpha  = 0f;
            if (prayingStatusCanvasGroup != null) prayingStatusCanvasGroup.alpha = 0f;
        }

        private void Update()
        {
            UpdateUIFade();

            if (_playerInZone && !_isPraying)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null && keyboard[Key.E].wasPressedThisFrame)
                    StartCoroutine(PraySequence());
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Trigger Detection
        // ─────────────────────────────────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInZone  = true;
            _promptVisible = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInZone  = false;
            _promptVisible = false;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Prayer Sequence — Master Coroutine
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator PraySequence()
        {
            _isPraying     = true;
            _promptVisible = false;

            // ── Bước 1: Khóa input player ────────────────────────────────────
            DisablePlayerControl();

            // ── Bước 2: Xoay người về phía tượng ────────────────────────────
            yield return StartCoroutine(RotatePlayerToBuddha());

            // ── Bước 3: Hiện UI "Đang cầu nguyện" + chuông ──────────────────
            if (prayingStatusCanvasGroup != null)
                prayingStatusCanvasGroup.alpha = 1f;

            if (backgroundAudioSource != null)
                backgroundAudioSource.Pause();

            // ── Bước 4: Camera ngẩng lên nhìn tượng Phật (pitch âm = nhìn lên)
            float targetLookUpPitch = -lookUpAngle;   // âm = nhìn lên trong Unity
            yield return StartCoroutine(AnimateCameraPitch(
                from:     _originalCinemachinePitch,
                to:       targetLookUpPitch,
                duration: lookUpDuration,
                easing:   EasingType.SmoothStep
            ));

            // ── Bước 5: Giữ nhìn tượng 3 giây ──────────────────────────────
            yield return new WaitForSeconds(lookUpHoldDuration);

            // ── Bước 6: Dập đầu N lần ────────────────────────────────────────
            for (int i = 0; i < bowCount; i++)
            {
                // Cúi xuống
                PlayTempleBell();
                yield return StartCoroutine(AnimateCameraPitch(
                    from:     targetLookUpPitch,
                    to:       bowAngle,
                    duration: bowDownDuration,
                    easing:   EasingType.EaseIn
                ));

                // Dừng nhỏ ở điểm cúi thấp nhất
                yield return new WaitForSeconds(bowHoldAtBottom);

                // Ngẩng lên — lần cuối về pitch = 0, các lần khác về targetLookUpPitch
                float returnTarget = (i == bowCount - 1) ? 0f : targetLookUpPitch;
                yield return StartCoroutine(AnimateCameraPitch(
                    from:     bowAngle,
                    to:       returnTarget,
                    duration: bowUpDuration,
                    easing:   EasingType.EaseOut
                ));
            }

            // ── Bước 7: Camera về vị trí trung lập ──────────────────────────
            yield return StartCoroutine(AnimateCameraPitch(
                from:     GetCurrentCameraPitch(),
                to:       0f,
                duration: returnDuration,
                easing:   EasingType.SmoothStep
            ));

            // ── Bước 8: Ẩn UI + mở lại input ────────────────────────────────
            if (prayingStatusCanvasGroup != null)
                prayingStatusCanvasGroup.alpha = 0f;

            if (backgroundAudioSource != null)
                backgroundAudioSource.UnPause();

            if (templeBellAudioSource != null)
                templeBellAudioSource.Stop();

            EnablePlayerControl();
            _isPraying = false;

            if (_playerInZone)
                _promptVisible = true;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Camera Pitch Animation
        // ─────────────────────────────────────────────────────────────────────

        private enum EasingType { Linear, SmoothStep, EaseIn, EaseOut }

        /// <summary>
        /// Animate camera pitch (localEulerAngles.x của PlayerCameraRoot).
        /// Pitch dương = nhìn xuống, âm = nhìn lên (chuẩn Unity).
        /// </summary>
        private IEnumerator AnimateCameraPitch(float from, float to,
                                               float duration, EasingType easing)
        {
            if (_cameraRoot == null || duration <= 0f)
            {
                ApplyCameraPitch(to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Easing curve
                switch (easing)
                {
                    case EasingType.SmoothStep: t = t * t * (3f - 2f * t);           break;
                    case EasingType.EaseIn:     t = t * t;                            break;
                    case EasingType.EaseOut:    t = 1f - (1f - t) * (1f - t);         break;
                }

                float pitch = Mathf.Lerp(from, to, t);
                ApplyCameraPitch(pitch);
                yield return null;
            }
            ApplyCameraPitch(to);
        }

        /// <summary>
        /// Áp pitch (độ) vào PlayerCameraRoot.localEulerAngles.x.
        /// FPC đã bị tắt nên có thể set trực tiếp.
        /// </summary>
        private void ApplyCameraPitch(float pitch)
        {
            if (_cameraRoot == null) return;
            var e = _cameraRoot.localEulerAngles;
            e.x = pitch;
            _cameraRoot.localEulerAngles = e;
        }

        /// <summary>Đọc pitch hiện tại từ localEulerAngles, chuẩn hoá về [-180, 180].</summary>
        private float GetCurrentCameraPitch()
        {
            if (_cameraRoot == null) return 0f;
            float raw = _cameraRoot.localEulerAngles.x;
            // Unity lưu euler 0-360, cần đưa về -180..+180
            return raw > 180f ? raw - 360f : raw;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Player Rotation Helper
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator RotatePlayerToBuddha()
        {
            if (playerObject == null || buddhaStatue == null) yield break;

            Vector3 dir = buddhaStatue.position - playerObject.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) yield break;

            Quaternion startRot  = playerObject.transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            float elapsed = 0f;

            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / rotationDuration);
                playerObject.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }
            playerObject.transform.rotation = targetRot;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Player Control Helpers
        // ─────────────────────────────────────────────────────────────────────

        private void DisablePlayerControl()
        {
            // Lưu pitch hiện tại trước khi khóa
            _originalCinemachinePitch = GetCurrentCameraPitch();

            if (_fpsController != null) _fpsController.enabled = false;

            if (_starterInputs != null)
            {
                _starterInputs.move   = Vector2.zero;
                _starterInputs.look   = Vector2.zero;
                _starterInputs.jump   = false;
                _starterInputs.sprint = false;
            }
            // KHÔNG thay đổi Cursor.lockState — tránh break Cinemachine Brain
        }

        private void EnablePlayerControl()
        {
            if (_fpsController != null) _fpsController.enabled = true;

            if (_starterInputs != null)
            {
                _starterInputs.look              = Vector2.zero;
                _starterInputs.cursorInputForLook = true;
                _starterInputs.cursorLocked      = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void OnEnable()
        {
            // Safety net: nếu domain reload xảy ra khi đang pray
            if (_isPraying)
            {
                _isPraying = false;
                EnablePlayerControl();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Audio Helper
        // ─────────────────────────────────────────────────────────────────────

        private void PlayTempleBell()
        {
            if (templeBellAudioSource == null || templeBellClip == null) return;
            templeBellAudioSource.clip = templeBellClip;
            templeBellAudioSource.Play();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UI Fade Helper
        // ─────────────────────────────────────────────────────────────────────

        private void UpdateUIFade()
        {
            if (prayPromptCanvasGroup == null) return;
            float target = (_promptVisible && !_isPraying) ? 1f : 0f;
            prayPromptCanvasGroup.alpha = Mathf.MoveTowards(
                prayPromptCanvasGroup.alpha, target, uiFadeSpeed * Time.deltaTime);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────────────

        public bool IsPraying => _isPraying;
    }
}
