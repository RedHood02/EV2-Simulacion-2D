using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Player Interaction")]
    public string playerTag = "Player";
    public float restartDelay = 2f;

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
        if (isDestroyed) return;

        ApplyGravityEffect();
        HandleAsteroidCollisions();
        transform.position += velocity * Time.deltaTime;

        UpdateHeatColor();
        CheckOverheat();
    }

    void ApplyGravityEffect()
    {
        if (Bullets == null) return;

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
        List<Asteroid> asteroids = new List<Asteroid>(AsteroidManager.instance.Asteroids);

        foreach (Asteroid other in asteroids)
        {
            if (other == null || other == this || other.isDestroyed) continue;

            Vector3 delta = other.transform.position - transform.position;
            float dist = delta.magnitude;
            float minDist = radius + other.radius;

            // --- Player collision check ---
            CheckPlayerCollision();

            if (dist < minDist && dist > 0f)
            {
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

                float v1nFinal = (v1n * (mass - other.mass) + 2f * other.mass * v2n) / (mass + other.mass);
                float v2nFinal = (v2n * (other.mass - mass) + 2f * mass * v1n) / (mass + other.mass);

                Vector3 v1nVec = v1nFinal * normal;
                Vector3 v1tVec = v1t * tangent;
                Vector3 v2nVec = v2nFinal * normal;
                Vector3 v2tVec = v2t * tangent;

                velocity = v1nVec + v1tVec;
                other.velocity = v2nVec + v2tVec;

                // --- Heat system ---
                HandleHeatTransfer(other);
            }
        }

        // Check player collision again outside asteroid loop (for safety)
        CheckPlayerCollision();
    }

    void CheckPlayerCollision()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        float playerRadius = 1f; // Adjust if you have a collider reference on the player

        if (dist < radius + playerRadius)
        {
            KillPlayer(player);
        }
    }

    void KillPlayer(GameObject player)
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Optional: add explosion effect on the player
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, player.transform.position, Quaternion.identity);

        // Disable player controls if script exists
        MonoBehaviour controlScript = player.GetComponent<MonoBehaviour>();
        if (controlScript != null) controlScript.enabled = false;

        // Destroy asteroid immediately (so it doesn’t keep colliding)
        StartCoroutine(RestartGameAfterDelay());
    }

    IEnumerator RestartGameAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);

        // Reload the current active scene
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    void HandleHeatTransfer(Asteroid other)
    {
        if (other == null) return;

        heat += heatIncreasePerCollision;
        other.heat += heatIncreasePerCollision;

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

        StartCoroutine(RemoveSafely());
    }

    private IEnumerator RemoveSafely()
    {
        yield return null;

        AsteroidManager manager = AsteroidManager.instance;
        if (manager != null && manager.Asteroids.Contains(this))
            manager.Asteroids.Remove(this);

        Destroy(gameObject);
    }

    public Vector3 GetGravityForce(BulletController _bullet)
    {
        if (_bullet == null) return Vector3.zero;

        Vector3 direction = transform.position - _bullet.transform.position;
        float distance = direction.magnitude;
        if (distance < 0.1f) return Vector3.zero;

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
