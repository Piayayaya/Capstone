using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Put this on the QUESTION MARK object.
/// Click = move Character to this object, then load scene.
[RequireComponent(typeof(RectTransform))]
public class PortalClickWalkAndLoad : MonoBehaviour, IPointerClickHandler
{
    [Header("Who walks")]
    public RectTransform character;           // drag the left character RectTransform

    [Header("Where to walk (defaults to me)")]
    public RectTransform destination;         // drag the ? RectTransform (or leave empty)

    [Header("Movement")]
    public float speed = 800f;                // UI pixels/second (1080x1920)
    public float stopDistance = 10f;          // arrival radius in pixels

    [Header("Scene")]
    public string sceneToLoad = "Japan_NameTheFlag";

    bool walking;
    bool loading;

    RectTransform portal;

    void Awake()
    {
        portal = GetComponent<RectTransform>();
        if (!destination) destination = portal; // default to this (question mark)
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!character || walking || loading) return;
        walking = true;
    }

    void Update()
    {
        if (!walking || loading || !character || !destination) return;

        // Convert destination to the character's parent space if needed
        Vector2 dest =
            (character.parent == destination.parent)
                ? destination.anchoredPosition
                : (Vector2)character.parent.InverseTransformPoint(destination.position);

        Vector2 cur = character.anchoredPosition;
        float step = speed * Time.unscaledDeltaTime;
        character.anchoredPosition = Vector2.MoveTowards(cur, dest, step);

        if (Vector2.Distance(character.anchoredPosition, dest) <= stopDistance)
        {
            walking = false;
            loading = true;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
