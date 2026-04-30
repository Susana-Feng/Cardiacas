using UnityEngine;

public class ContainerChecker : MonoBehaviour
{
    public int requiredCount = 3;
    private int currentCount = 0;
    private bool isFull = false;

    private void OnTriggerEnter(Collider other)
    {
        // Adjust the tag to match your objects
        if (other.CompareTag("PlaceableObject"))
        {
            currentCount++;
            CheckIfFull();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlaceableObject"))
        {
            currentCount--;
            if (isFull)
            {
                isFull = false;
                GameManager.Instance.OnContainerEmptied();
            }
        }
    }

    private void CheckIfFull()
    {
        if (!isFull && currentCount >= requiredCount)
        {
            isFull = true;
            GameManager.Instance.OnContainerFilled();
        }
    }
}