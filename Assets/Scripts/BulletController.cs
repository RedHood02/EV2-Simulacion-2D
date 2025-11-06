using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] float speed;
    public float mass = 1f;
    public float volume = 0.5f;
    [SerializeField] Vector3 velocity;
    Vector3 currentGravityForce;
    public List<Asteroid> currAsteroids = new List<Asteroid>();

    [Header("agua")]
    public Water water;
    [SerializeField] SpriteRenderer sr;

    public float dampingInWater = 0.9f;
    public float dampingInAir = 0.999f;

    private void Awake()
    {
        AsteroidManager.instance.GetBullet(this);
        water = Water.instance;
        Destroy(gameObject, 6f);
    }

    private void Start()
    {
        velocity = transform.up * speed;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // --- Gravity and movement ---
        CalculateCurrGravity();
        velocity += currentGravityForce * dt;
        transform.position += velocity * dt;
        transform.up = velocity.normalized;

        // --- Collision with asteroids ---
        HandleAsteroidCollisions();

        // --- Buoyancy in water ---
        HandleBuoyancy(dt);
    }

    private void HandleAsteroidCollisions()
    {
        // Copy list to avoid breaking the iteration if asteroids are removed
        List<Asteroid> asteroids = new List<Asteroid>(AsteroidManager.instance.Asteroids);

        foreach (Asteroid asteroid in asteroids)
        {
            if (asteroid == null) continue;

            float distance = Vector3.Distance(asteroid.transform.position, transform.position);
            if (distance < asteroid.radius)
            {
                // Safely remove from manager (if still exists)
                AsteroidManager manager = AsteroidManager.instance;
                if (manager != null && manager.Asteroids.Contains(asteroid))
                    manager.Asteroids.Remove(asteroid);

                // Create explosion or heat effect if desired
                if (asteroid.explosionPrefab != null)
                    Instantiate(asteroid.explosionPrefab, asteroid.transform.position, Quaternion.identity);

                Destroy(asteroid.gameObject);
                Destroy(gameObject);
                return; // stop processing after destruction
            }
        }
    }

    private void HandleBuoyancy(float dt)
    {
        if (sr == null || water == null) return;

        Bounds ball = sr.bounds;
        Bounds waterBounds = water.WorldBounds;

        // Check intersection
        if (ball.Intersects(waterBounds))
        {
            float overlapBottom = Mathf.Max(ball.min.y, waterBounds.min.y);
            float overlapTop = Mathf.Min(ball.max.y, waterBounds.max.y);
            float overlapHeight = Mathf.Max(0f, overlapTop - overlapBottom);

            float overlapLeft = Mathf.Max(ball.min.x, waterBounds.min.x);
            float overlapRight = Mathf.Min(ball.max.x, waterBounds.max.x);
            float overlapWidth = Mathf.Max(0f, overlapRight - overlapLeft);

            float fracVertical = Mathf.Clamp01(overlapHeight / ball.size.y);
            float fracHorizontal = Mathf.Clamp01(overlapWidth / ball.size.x);

            float submergedFraction = Mathf.Clamp01(fracVertical * fracHorizontal);
            float displacedVolume = volume * submergedFraction;

            float buoyantMag = water.density * displacedVolume;
            Vector3 buoyantForce = new Vector3(0f, buoyantMag);
            velocity += buoyantForce;

            // Apply damping inside water
            velocity *= Mathf.Clamp01(Mathf.Lerp(1f, dampingInWater, submergedFraction));
        }

        // Apply position update
        transform.position += velocity * dt;
    }

    public void CalculateCurrGravity()
    {
        currentGravityForce = Vector2.zero;

        // Defensive iteration (skip destroyed or null asteroids)
        foreach (Asteroid asteroid in AsteroidManager.instance.Asteroids)
        {
            if (asteroid == null || asteroid.gameObject == null)
                continue;

            Vector3 gravityForce = asteroid.GetGravityForce(this);
            currentGravityForce += gravityForce;
        }
    }
}
