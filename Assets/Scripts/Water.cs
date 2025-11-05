using UnityEngine;

public class Water : Singleton<Water>
{
    [SerializeField] SpriteRenderer sr;
    public float density = 1f;

    protected override bool persistent => false;

    public Bounds WorldBounds => sr.bounds;

    public float SurfaceY => sr.bounds.max.y;
}
