using UnityEngine;

public class CameraPath : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 5f;
    private Rigidbody rb;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //se o alvo nao for definido, nao faz nada
        if (target == null)
        {
            return;
        }
        Vector3 direction = (target.position - rb.position).normalized;

        Vector3 targetVelocity = direction * speed;

        rb.linearVelocity = targetVelocity;
    }
}