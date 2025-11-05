using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public List<BulletController> Bullets;

    public float gravitationalConstant = 0.5f;
    public float mass = 100f;
    public float gravityRadius = 50f;
    public float radius = 5f;

    bool inGravity;

    Vector3 velocity;
    [SerializeField] float speed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AsteroidManager.instance.Asteroids.Add(this);
        velocity = -transform.right * speed;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (BulletController bullet in Bullets)
        {
            Vector3 direction = transform.position - bullet.transform.position;
            float distance = direction.magnitude;

            if (distance < gravityRadius && distance > 0.1f)
            {
                if (!inGravity)
                {
                    inGravity = true;
                    bullet.currAsteroids.Add(this);
                }
            }
            else
            {
                if (inGravity)
                {
                    inGravity = false;
                    bullet.currAsteroids.Remove(this);
                }
            }
        }

        transform.position += velocity * Time.deltaTime;
    }

    public Vector3 GetGravityForce(BulletController _bullet)
    {
        Vector3 direction = transform.position - _bullet.transform.position;
        float distance = direction.magnitude;
        float forceMagnitude = gravitationalConstant * (mass * _bullet.mass) / (distance * distance);
        Vector3 force = direction.normalized * forceMagnitude;
        return force;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gravityRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
