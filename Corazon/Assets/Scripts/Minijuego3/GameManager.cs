using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int totalContainers = 3;
    public Transform door;
    public float doorSpeed = 2f;

    private int filledContainers = 0;
    private bool gameWon = false;
    private Vector3 doorOpenPos = new Vector3(13.24f, -0.02f, -18.28f);



    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (gameWon)
        {
            door.position = Vector3.MoveTowards(door.position, doorOpenPos, Time.deltaTime * doorSpeed);
        }
    }

    public void OnContainerFilled()
    {
        filledContainers++;
        if (filledContainers >= totalContainers)
        {
            gameWon = true;
            Debug.Log("You win! Door opening.");
        }
    }

    public void OnContainerEmptied()
    {
        filledContainers = Mathf.Max(0, filledContainers - 1);
        gameWon = false;
    }
}