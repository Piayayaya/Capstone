using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// Attach to an empty GameObject under your Canvas (e.g., "SeeItRoundController")
public class SeeItRoundController : MonoBehaviour
{
    [Header("UI (TMP counter)")]
    [SerializeField] TMP_Text counterText;          // e.g., a TMP object that starts as "0/3"

    [Header("Optional FX")]
    [SerializeField] bool showCheckOnFound = false; // show a small ✓ or pop on found
    [SerializeField] GameObject checkPrefab;        // optional: UI prefab parented under this controller

    [Header("Differences")]
    [SerializeField] List<DifferencePair> pairs = new List<DifferencePair>();

    [Header("Events")]
    public UnityEvent OnAllFound;                   // fire when all pairs are found

    int foundCount;

    void Awake()
    {
        // Bind targets to the controller
        foreach (var p in pairs)
        {
            if (p == null) continue;
            p.Init(this);
        }
    }

    void Start()
    {
        foundCount = 0;
        UpdateCounter();
    }

    // Called by DifferenceTarget when a pair is found (tapping either top or bottom)
    public void ReportPairFound(DifferencePair pair, Vector3 hitWorldPos)
    {
        if (pair == null || pair.Found) return;

        pair.MarkFound();

        foundCount++;
        UpdateCounter();

        if (showCheckOnFound && checkPrefab != null)
        {
            Instantiate(checkPrefab, hitWorldPos, Quaternion.identity, transform);
        }

        if (foundCount >= pairs.Count)
        {
            OnAllFound?.Invoke();
        }
    }

    void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = $"{foundCount}/{pairs.Count}";
    }

    // ---------- Helper Types ----------
    [System.Serializable]
    public class DifferencePair
    {
        [Tooltip("Clickable UI object in the TOP picture (e.g., apple top). Can be null if the item only exists on bottom.")]
        public GameObject topTarget;

        [Tooltip("Clickable UI object in the BOTTOM picture (e.g., apple bottom). Can be null if the item only exists on top.")]
        public GameObject bottomTarget;

        [HideInInspector] public bool Found;

        SeeItRoundController controller;
        DifferenceTarget topComp, botComp;

        public void Init(SeeItRoundController ctrl)
        {
            controller = ctrl;

            if (topTarget != null)
            {
                topComp = EnsureTargetComp(topTarget);
                topComp.Bind(this, controller);
            }

            if (bottomTarget != null)
            {
                botComp = EnsureTargetComp(bottomTarget);
                botComp.Bind(this, controller);
            }
        }

        public void MarkFound()
        {
            Found = true;

            // Prevent further taps and (by default) hide both sides of the pair
            if (topTarget)
            {
                var img = topTarget.GetComponent<Image>();
                if (img) img.raycastTarget = false;
                topTarget.SetActive(false);
            }
            if (bottomTarget)
            {
                var img = bottomTarget.GetComponent<Image>();
                if (img) img.raycastTarget = false;
                bottomTarget.SetActive(false);
            }
        }

        static DifferenceTarget EnsureTargetComp(GameObject go)
        {
            var t = go.GetComponent<DifferenceTarget>();
            if (t == null) t = go.AddComponent<DifferenceTarget>();
            return t;
        }
    }
}

/// Auto-added to each clickable Image in a pair.
/// Requires EventSystem + Canvas (GraphicRaycaster).
public class DifferenceTarget : MonoBehaviour, IPointerClickHandler
{
    SeeItRoundController.DifferencePair pair;
    SeeItRoundController controller;

    public void Bind(SeeItRoundController.DifferencePair p, SeeItRoundController c)
    {
        pair = p;
        controller = c;

        // Ensure UI Image receives clicks
        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (pair == null || controller == null || pair.Found) return;

        controller.ReportPairFound(pair, transform.position);
    }
}
