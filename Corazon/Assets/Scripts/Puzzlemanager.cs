using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optional manager — place on any GameObject in the scene.
/// Drag all your PuzzleSnapController slots into the array.
/// OnPuzzleComplete fires when every slot is solved.
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    [Header("All Slots in This Puzzle")]
    public PuzzleSnapController[] slots;

    [Header("Completion Event")]
    public UnityEvent OnPuzzleComplete;

    private bool _completed = false;

    private void Update()
    {
        if (_completed) return;

        foreach (var slot in slots)
        {
            if (!slot.IsSolved) return; // at least one unsolved → not done
        }

        _completed = true;
        Debug.Log("[Puzzle] 🎉 Puzzle complete!");
        OnPuzzleComplete?.Invoke();
    }
}