using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Vector3 velocity;

    private void Start()
    {
        velocity = transform.up * speed;
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }
}
