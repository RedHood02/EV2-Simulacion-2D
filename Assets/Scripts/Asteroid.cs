using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public List<BulletController> Bullets;

    [Header("Gravity Settings")]
    public float gravitationalConstant = 0.5f;
    public float mass = 100f;
    public float gravityRadius = 50f;

    [Header("Movement & Collision")]
    public float radius = 5f;
    [SerializeField] float speed = 5f;
    [SerializeField] float verticalVariation = 0.3f;

    [Header("Heat Settings")]
    public float heat = 0f;
    public float heatIncreasePerCollision = 10f;
    public float heatTransferRate = 0.3f;
    public float maxHeat = 100f;
    public Color baseColor = Color.gray;
    public Color overheatColor = Color.red;
    public GameObject explosionPrefab;

    private SpriteRenderer spriteRenderer;
    private Vector3 velocity;
    private bool inGravity;
    private float verticalDirection;
    private bool isDestroyed; // flag to mark pending destruction

    void Start()
    {
        // Register asteroid
        AsteroidManager.instance.Asteroids.Add(this);

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"Asteroid {name} missing SpriteRenderer!");
        }

        // Randomize movement slightly upward/downward
        verticalDirection = Random.value > 0.5f ? 1f : -1f;
        float randomVertical = Random.Range(0f, verticalVariation) * verticalDirection;
        Vector3 direction = new Vector3(-1f, randomVertical, 0f).normalized;
        velocity = direction * speed;
    }

    void Update()
    {
        if (isDestroyed) return; // skip if marked for destruction

        ApplyGravityEffect();
        HandleAsteroidCollisions();
        transform.position += velocity * Time.deltaTime;

        UpdateHeatColor();
        CheckOverheat();
    }

    void ApplyGravityEffect()
    {
        foreach (BulletController bullet in Bullets)
        {
            if (bullet == null) continue;

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
    }

    void HandleAsteroidCollisions()
    {
        // Copy list to avoid modification errors if any asteroid explodes during iteration
        List<Asteroid> asteroids = new List<Asteroid>(AsteroidManager.instance.Asteroids);

        foreach (Asteroid other in asteroids)
        {
            if (other == null || other == this || other.isDestroyed) continue;

            Vector3 delta = other.transform.position - transform.position;
            float dist = delta.magnitude;
            float minDist = radius + other.radius;

            if (dist < minDist && dist > 0f)
            {
                // Separate to prevent overlap
                float overlap = 0.5f * (minDist - dist);
                Vector3 correction = delta.normalized * overlap;
                transform.position -= correction;
                other.transform.position += correction;

                // --- Elastic collision physics ---
                Vector3 normal = delta.normalized;
                Vector3 tangent = new Vector3(-normal.y, normal.x, 0f);

                float v1n = Vector3.Dot(normal, velocity);
                float v1t = Vector3.Dot(tangent, velocity);
                float v2n = Vector3.Dot(normal, other.velocity);
                float v2t = Vector3.Dot(tangent, other.velocity);

                float v1tFinal = v1t;
                float v2tFinal = v2t;

                float v1nFinal = (v1n * (mass - other.mass) + 2f * other.mass * v2n) / (mass + other.mass);
                float v2nFinal = (v2n * (other.mass - mass) + 2f * mass * v1n) / (mass + other.mass);

                Vector3 v1nVec = v1nFinal * normal;
                Vector3 v1tVec = v1tFinal * tangent;
                Vector3 v2nVec = v2nFinal * normal;
                Vector3 v2tVec = v2tFinal * tangent;

                velocity = v1nVec + v1tVec;
                other.velocity = v2nVec + v2tVec;

                // --- Heat system ---
                HandleHeatTransfer(other);
            }
        }
    }

    void HandleHeatTransfer(Asteroid other)
    {
        if (other == null) return;

        // Add base heat from collision impact
        heat += heatIncreasePerCollision;
        other.heat += heatIncreasePerCollision;

        // Transfer heat between asteroids
        if (heat > other.heat)
        {
            float transfer = (heat - other.heat) * heatTransferRate;
            heat -= transfer;
            other.heat += transfer;
        }
        else if (other.heat > heat)
        {
            float transfer = (other.heat - heat) * heatTransferRate;
            other.heat -= transfer;
            heat += transfer;
        }
    }

    void UpdateHeatColor()
    {
        if (spriteRenderer == null) return;
        float t = Mathf.Clamp01(heat / maxHeat);
        spriteRenderer.color = Color.Lerp(baseColor, overheatColor, t);
    }

    void CheckOverheat()
    {
        if (heat >= maxHeat)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // Delay removal to avoid mid-loop modification errors
        StartCoroutine(RemoveSafely());
    }

    private System.Collections.IEnumerator RemoveSafely()
    {
        yield return null; // wait one frame to ensure all collision loops finish

        AsteroidManager manager = AsteroidManager.instance;
        if (manager != null && manager.Asteroids.Contains(this))
            manager.Asteroids.Remove(this);

        Destroy(gameObject);
    }

    public Vector3 GetGravityForce(BulletController _bullet)
    {
        Vector3 direction = transform.position - _bullet.transform.position;
        float distance = direction.magnitude;
        float forceMagnitude = gravitationalConstant * (mass * _bullet.mass) / (distance * distance);
        return direction.normalized * forceMagnitude;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gravityRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
