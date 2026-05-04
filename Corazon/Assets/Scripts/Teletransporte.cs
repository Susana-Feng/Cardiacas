using UnityEngine;
public class Teletransportacion : MonoBehaviour
{
    public Transform Target;
    public GameObject ThePlayer;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ThePlayer)
        {
            ThePlayer.transform.position = Target.transform.position;
        }
    }
}