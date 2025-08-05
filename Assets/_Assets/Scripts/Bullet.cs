using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 10f;
    public float lifetime = 2f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 direction)
    {
        // Set the bullet's velocity immediately based on the provided direction
        rb.velocity = direction.normalized * bulletSpeed;

        // Destroy the bullet after a set time to clean up the scene
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy the bullet when it collides with another object
        Destroy(gameObject);
    }
}
