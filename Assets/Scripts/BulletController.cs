using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] float speed;
    public float mass = 1f;
    [SerializeField] Vector3 velocity;
    Vector3 currentGravityForce;
    public List<Asteroid> currAsteroids = new List<Asteroid>();

    private void Awake()
    {
        AsteroidManager.instance.GetBullet(this);
    }

    private void Start()
    {
        velocity = transform.up * speed;
    }

    private void Update()
    {
        CalculateCurrGravity();
        velocity += currentGravityForce * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        transform.up = velocity.normalized;
    }

    public void CalculateCurrGravity()
    {
        currentGravityForce = Vector2.zero;
        foreach (Asteroid x in currAsteroids)
        {
            Vector3 gravityForce = x.GetGravityForce(this);
            Debug.Log(gravityForce);
            currentGravityForce += gravityForce;
        }
    }
}
