using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    [Header("All Slots in This Puzzle")]
    public PuzzleSnapController[] slots;

    [Header("Door Settings")]
    public GameObject door;
    public Vector3 targetPosition;
    public float doorSpeed = 2f;

    [Header("Objects To Hide On Completion")]
    public GameObject[] objectsToDisappear;

    [Header("Objects To Show On Completion")]
    public GameObject[] objectsToAppear;

    [Header("Completion Event")]
    public UnityEvent OnPuzzleComplete;

    private bool _completed = false;
    private bool _doorMoving = false;

    private void Start()
    {
        if (objectsToAppear != null)
            foreach (var obj in objectsToAppear)
                if (obj != null) obj.SetActive(false);
    }

    private void Update()
    {
        if (!_completed)
        {
            foreach (var slot in slots)
                if (!slot.IsSolved) return;

            _completed = true;
            _doorMoving = true;
            Debug.Log("[Puzzle] 🎉 Puzzle complete!");

            StartCoroutine(CompletionSequence());
            OnPuzzleComplete?.Invoke();
        }

        if (_doorMoving && door != null)
        {
            door.transform.position = Vector3.MoveTowards(
                door.transform.position,
                targetPosition,
                doorSpeed * Time.deltaTime
            );

            if (Vector3.Distance(door.transform.position, targetPosition) < 0.001f)
            {
                door.transform.position = targetPosition;
                _doorMoving = false;
                Debug.Log("[Puzzle] 🚪 Door opened!");
            }
        }
    }

    private IEnumerator CompletionSequence()
    {
        yield return new WaitForSeconds(1f);
        HideObjects();
        ShowObjects();
    }

    private void HideObjects()
    {
        if (objectsToDisappear == null) return;

        foreach (var obj in objectsToDisappear)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"[Puzzle] 👻 Hidden: {obj.name}");
            }
        }
    }

    private void ShowObjects()
    {
        if (objectsToAppear == null) return;

        foreach (var obj in objectsToAppear)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log($"[Puzzle] ✨ Shown: {obj.name}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (door == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetPosition, 0.15f);
        Gizmos.DrawLine(door.transform.position, targetPosition);
    }
}