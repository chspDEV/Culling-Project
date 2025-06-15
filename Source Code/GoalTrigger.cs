
using UnityEngine;
using System; 

public class GoalTrigger : MonoBehaviour
{
    public static event Action OnCameraReachedGoal;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            Debug.Log("Cheguei no alvo");
            OnCameraReachedGoal?.Invoke();
        }
    }
}