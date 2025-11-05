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
        Destroy(gameObject, 10);
    }

    private void Start()
    {
        velocity = transform.up * speed;
    }

    private void Update()
    {
        //gravedad y movimiento
        CalculateCurrGravity();
        velocity += currentGravityForce * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        transform.up = velocity.normalized;

        //colision con asteroides
        currAsteroids = AsteroidManager.instance.Asteroids;
        foreach(Asteroid x in currAsteroids)
        {
            if(Vector3.Distance(x.transform.position, transform.position) < x.radius)
            {
                AsteroidManager.instance.Asteroids.Remove(x);
                Destroy(x.gameObject);
                Destroy(this.gameObject);
            }
        }

        //flotabilidad en el agua

        float dt = Time.deltaTime;
        Bounds ball = sr.bounds;
        Bounds waterBounds = water.WorldBounds;

        // ¿Hay intersección entre agua y pelota?
        if (ball.Intersects(waterBounds))
        {
            // Solapamiento vertical
            float overlapBottom = Mathf.Max(ball.min.y, waterBounds.min.y);
            float overlapTop = Mathf.Min(ball.max.y, waterBounds.max.y);
            float overlapHeight = Mathf.Max(0f, overlapTop - overlapBottom);

            // Solapamiento horizontal
            float overlapLeft = Mathf.Max(ball.min.x, waterBounds.min.x);
            float overlapRight = Mathf.Min(ball.max.x, waterBounds.max.x);
            float overlapWidth = Mathf.Max(0f, overlapRight - overlapLeft);

            float fracVertical = Mathf.Clamp01(overlapHeight / ball.size.y);
            float fracHorizontal = Mathf.Clamp01(overlapWidth / ball.size.x);

            // Fracción sumergida aproximada por área de bounds (mejor que solo vertical)
            float submergedFraction = Mathf.Clamp01(fracVertical * fracHorizontal);

            // Volumen desplazado (Arquímedes)
            float displacedVolume = volume * submergedFraction;

            // Empuje: Fb = ? * V_desplazado * g  (hacia arriba)
            float buoyantMag = water.density * displacedVolume;
            Vector3 buoyantForce = new Vector3(0f, buoyantMag);

            velocity += buoyantForce;

            // Amortiguación dentro del agua (aplicada a la velocidad)
            velocity *= Mathf.Clamp01(Mathf.Lerp(1f, dampingInWater, submergedFraction));
        }

        transform.position += (velocity * dt);
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
