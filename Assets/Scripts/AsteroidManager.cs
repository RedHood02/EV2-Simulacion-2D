using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : Singleton<AsteroidManager>
{
    public List<Asteroid> Asteroids;

    protected override bool persistent => false;

    public void GetBullet(BulletController bullet)
    {
        foreach (Asteroid b in Asteroids)
        {
            b.Bullets.Add(bullet);
        }
    }
}
