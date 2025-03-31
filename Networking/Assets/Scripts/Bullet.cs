using UnityEngine;

public class Bullet : MonoBehaviour
{
    public event System.Action<GameObject> OnBulletHit;

    [SerializeField] private bool isPlayerBullet;
    [SerializeField] private float damage;
    [SerializeField] private float lifespan;

    private Rigidbody2D rig;
    private float lifespanTimer;

    private void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (lifespanTimer > 0)
        {
            lifespanTimer -= Time.fixedDeltaTime;
            if (lifespanTimer <= 0)
            {
                OnBulletHit?.Invoke(gameObject);
            }
        }
    }

    public void Fire(Vector2 position, Vector2 velocity)
    {
        transform.position = position;
        rig.linearVelocity = velocity;
        lifespanTimer = lifespan;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPlayerBullet)
        {
            if (collision.CompareTag("Client"))
            {
                collision.GetComponent<Health>().TakeDamage(damage);
                OnBulletHit?.Invoke(gameObject);
            }
        }
        else
        {
            if (collision.CompareTag("Client"))
            {
                collision.GetComponent<Health>().TakeDamage(damage);
                OnBulletHit?.Invoke(gameObject);
            }
        }

        if (collision.CompareTag("Target"))
        {
            collision.GetComponent<Target>().HitTarget();
            OnBulletHit?.Invoke(gameObject);
        }
    }

    private void OnProjectileHit()
    {
        OnBulletHit?.Invoke(gameObject);
    }
}