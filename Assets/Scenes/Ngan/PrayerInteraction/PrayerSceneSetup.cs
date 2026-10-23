using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BuddhistTemple
{
    /// <summary>
    /// PrayerSceneSetup — Editor-time and runtime scene bootstrapper.
    ///
    /// When placed in the scene (or run via ExecuteAlways), this script
    /// automatically creates:
    ///   • Prayer trigger zone (BoxCollider) in front of the Buddha statue
    ///   • World-space Canvas with "Press E to Pray" prompt (fade-in/out)
    ///   • "Praying…" status panel
    ///   • AudioSource on the trigger object
    ///   • Wires all references into PrayerInteractionSystem
    ///
    /// Attach this script to any empty GameObject and hit Play once.
    /// After setup it disables itself so it doesn't repeat.
    /// </summary>
    [AddComponentMenu("Buddhist Temple/Prayer Scene Setup")]
    public class PrayerSceneSetup : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────────────
        //  Inspector
        // ─────────────────────────────────────────────────────────────────────

        [Header("Scene References (auto-found if empty)")]
        public Transform buddhaStatue;
        public GameObject playerObject;

        [Header("Trigger Zone")]
        [Tooltip("Size of the prayer trigger box.")]
        public Vector3 triggerSize = new Vector3(4f, 2.5f, 3f);

        [Tooltip("How far in front of the Buddha the trigger zone is placed.")]
        public float triggerForwardOffset = 4.5f;

        [Header("UI Settings")]
        public Color promptTextColor    = new Color(1f, 0.93f, 0.55f, 1f); // golden
        public Color prayingTextColor   = new Color(0.55f, 0.85f, 1f, 1f); // light blue
        public int   promptFontSize     = 36;
        public int   prayingFontSize    = 30;

        // ─────────────────────────────────────────────────────────────────────
        //  Constants
        // ─────────────────────────────────────────────────────────────────────

        private const string PRAYER_ROOT_NAME   = "PrayerSystem";
        private const string TRIGGER_NAME       = "PrayerTriggerZone";
        private const string CANVAS_NAME        = "PrayerUI_Canvas";
        private const string PROMPT_PANEL_NAME  = "PromptPanel";
        private const string PRAYING_PANEL_NAME = "PrayingPanel";

        // ─────────────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────────────

        private void Start()
        {
            // Skip if already set up
            if (GameObject.Find(PRAYER_ROOT_NAME) != null)
            {
                Debug.Log("[PrayerSceneSetup] Prayer system already exists in scene — skipping setup.");
                return;
            }

            AutoFindReferences();
            BuildPrayerSystem();

            // Self-disable after one-time setup
            enabled = false;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Setup
        // ─────────────────────────────────────────────────────────────────────

        private void AutoFindReferences()
        {
            if (buddhaStatue == null)
            {
                var obj = GameObject.Find("seap_model-tuong-phat");
                if (obj != null) buddhaStatue = obj.transform;
            }

            if (playerObject == null)
                playerObject = GameObject.FindGameObjectWithTag("Player");

            if (buddhaStatue == null)
                Debug.LogWarning("[PrayerSceneSetup] Could not find Buddha statue. Assign it manually.");

            if (playerObject == null)
                Debug.LogWarning("[PrayerSceneSetup] Could not find Player. Assign it manually.");
        }

        private void BuildPrayerSystem()
        {
            // Root container
            var root = new GameObject(PRAYER_ROOT_NAME);

            // ── 1. Trigger Zone ──────────────────────────────────────────────
            var triggerGO = BuildTriggerZone(root.transform);

            // ── 2. UI Canvas ─────────────────────────────────────────────────
            CanvasGroup promptCG, prayingCG;
            BuildUICanvas(root.transform, out promptCG, out prayingCG);

            // ── 3. Audio Source ──────────────────────────────────────────────
            var audioSource = triggerGO.AddComponent<AudioSource>();
            audioSource.playOnAwake  = false;
            audioSource.spatialBlend = 0f;        // 2-D UI sound
            audioSource.volume       = 0.85f;

            // ── 4. Prayer Interaction System ─────────────────────────────────
            var pis = triggerGO.AddComponent<PrayerInteractionSystem>();
            pis.buddhaStatue             = buddhaStatue;
            pis.playerObject             = playerObject;
            pis.templeBellAudioSource    = audioSource;
            pis.prayPromptCanvasGroup    = promptCG;
            pis.prayingStatusCanvasGroup = prayingCG;
            pis.rotationDuration         = 0.6f;
            // prayDuration đã được thay bằng lookUpHoldDuration + bowCount * bowCycle
            pis.uiFadeSpeed              = 5.0f;


            Debug.Log("[PrayerSceneSetup] ✓ Prayer system created successfully.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Trigger Zone Builder
        // ─────────────────────────────────────────────────────────────────────

        private GameObject BuildTriggerZone(Transform parent)
        {
            var go = new GameObject(TRIGGER_NAME);
            go.transform.SetParent(parent, false);

            // Position: in front of the Buddha (along world -Z relative to statue facing)
            if (buddhaStatue != null)
            {
                // The Buddha faces roughly -X (Y rotation ≈ 88°), so "in front" = -X
                // We use Vector3.forward relative to statue's facing direction
                Vector3 buddhaFacing = buddhaStatue.forward;
                // If the model was imported rotated 270 on X, treat it as facing -Z in world
                // We'll just place the trigger along the vector from statue toward player
                Vector3 toPlayer = playerObject != null
                    ? (playerObject.transform.position - buddhaStatue.position).normalized
                    : -Vector3.forward;
                toPlayer.y = 0f;
                toPlayer.Normalize();

                Vector3 pos = buddhaStatue.position
                              + toPlayer * triggerForwardOffset
                              + Vector3.up * (triggerSize.y * 0.5f - buddhaStatue.position.y * 0.5f);
                pos.y = 1.25f;  // ground-level center height
                go.transform.position = pos;
            }

            // Box Collider (trigger)
            var col = go.AddComponent<BoxCollider>();
            col.size      = triggerSize;
            col.center    = Vector3.zero;
            col.isTrigger = true;

            // Visual gizmo tag
            go.tag = "Untagged";

            return go;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UI Canvas Builder
        // ─────────────────────────────────────────────────────────────────────

        private void BuildUICanvas(Transform parent, out CanvasGroup promptCG, out CanvasGroup prayingCG)
        {
            // ── Canvas ───────────────────────────────────────────────────────
            var canvasGO = new GameObject(CANVAS_NAME);
            canvasGO.transform.SetParent(parent, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            // ── Prompt Panel ("Press E to Pray") ─────────────────────────────
            promptCG = BuildPromptPanel(canvasGO.transform);

            // ── Praying Panel ("Praying...") ──────────────────────────────────
            prayingCG = BuildPrayingPanel(canvasGO.transform);
        }

        /// <summary>Creates the "Press E to Pray" panel with golden text + animated border.</summary>
        private CanvasGroup BuildPromptPanel(Transform canvasTransform)
        {
            // Panel root
            var panelGO = new GameObject(PROMPT_PANEL_NAME);
            panelGO.transform.SetParent(canvasTransform, false);

            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.1f);
            panelRect.anchorMax = new Vector2(0.5f, 0.1f);
            panelRect.pivot     = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(420f, 80f);
            panelRect.anchoredPosition = Vector2.zero;

            // Background image (semi-transparent dark panel)
            var bgImage = panelGO.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.03f, 0.02f, 0.75f);

            var cg = panelGO.AddComponent<CanvasGroup>();
            cg.alpha          = 0f;
            cg.blocksRaycasts = false;

            // Golden border (child image)
            BuildBorderFrame(panelGO.transform, new Color(0.85f, 0.68f, 0.2f, 0.9f));

            // Icon (lotus / prayer hands emoji fallback)
            BuildIconText(panelGO.transform, "🙏", new Vector2(-140f, 0f), 28);

            // Main text
            BuildText(
                panelGO.transform,
                "prompt_text",
                "Press  E  to Pray",
                promptTextColor,
                promptFontSize,
                new Vector2(20f, 0f),
                new Vector2(300f, 60f),
                FontStyle.Bold
            );

            return cg;
        }

        /// <summary>Creates the "Praying..." panel shown during the prayer sequence.</summary>
        private CanvasGroup BuildPrayingPanel(Transform canvasTransform)
        {
            var panelGO = new GameObject(PRAYING_PANEL_NAME);
            panelGO.transform.SetParent(canvasTransform, false);

            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.88f);
            panelRect.anchorMax = new Vector2(0.5f, 0.88f);
            panelRect.pivot     = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(340f, 70f);
            panelRect.anchoredPosition = Vector2.zero;

            var bgImage = panelGO.AddComponent<Image>();
            bgImage.color = new Color(0.03f, 0.05f, 0.1f, 0.80f);

            var cg = panelGO.AddComponent<CanvasGroup>();
            cg.alpha          = 0f;
            cg.blocksRaycasts = false;

            BuildBorderFrame(panelGO.transform, new Color(0.4f, 0.75f, 1f, 0.85f));

            BuildIconText(panelGO.transform, "☸", new Vector2(-110f, 0f), 26);

            BuildText(
                panelGO.transform,
                "praying_text",
                "Praying…",
                prayingTextColor,
                prayingFontSize,
                new Vector2(20f, 0f),
                new Vector2(240f, 55f),
                FontStyle.Italic
            );

            return cg;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UI Component Helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Adds a thin border-rect child image for visual frame.</summary>
        private void BuildBorderFrame(Transform parent, Color color)
        {
            var borderGO = new GameObject("border_frame");
            borderGO.transform.SetParent(parent, false);

            var rt = borderGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-2f, -2f);
            rt.offsetMax = new Vector2( 2f,  2f);

            var img = borderGO.AddComponent<Image>();
            img.color = color;

            // Move border behind text by adjusting sibling index to 0
            borderGO.transform.SetAsFirstSibling();
        }

        /// <summary>Creates a Text child for icon/emoji symbols.</summary>
        private void BuildIconText(Transform parent, string icon, Vector2 anchoredPos, int size)
        {
            var go = new GameObject("icon");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin       = new Vector2(0.5f, 0.5f);
            rt.anchorMax       = new Vector2(0.5f, 0.5f);
            rt.pivot           = new Vector2(0.5f, 0.5f);
            rt.sizeDelta       = new Vector2(50f, 50f);
            rt.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.text      = icon;
            text.fontSize  = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color     = Color.white;
        }

        /// <summary>Creates a legacy Text label child.</summary>
        private void BuildText(Transform parent, string name, string content,
                               Color color, int fontSize, Vector2 anchoredPos,
                               Vector2 sizeDelta, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin       = new Vector2(0.5f, 0.5f);
            rt.anchorMax       = new Vector2(0.5f, 0.5f);
            rt.pivot           = new Vector2(0.5f, 0.5f);
            rt.sizeDelta       = sizeDelta;
            rt.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.text      = content;
            text.fontSize  = fontSize;
            text.fontStyle = style;
            text.color     = color;
            text.alignment = TextAnchor.MiddleCenter;
        }
    }
}
