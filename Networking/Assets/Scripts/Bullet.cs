using UnityEngine;

public class Bullet : MonoBehaviour
{
    public event System.Action<GameObject> OnBulletHit;

    [SerializeField] private bool isPlayerBullet;
    [SerializeField] private float damage;
    [SerializeField] private float lifespan;

    private Rigidbody2D rig;
    private float lifespanTimer;
    private int ownerId;

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

    public void Fire(int ownerId, Vector2 position, Vector2 velocity)
    {
        this.ownerId = ownerId;
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
                collision.GetComponent<Health>().TakeDamage(damage, ownerId);
                OnBulletHit?.Invoke(gameObject);
            }
        }
        else
        {
            if (collision.CompareTag("Player"))
            {
                collision.GetComponent<Health>().TakeDamage(damage, ownerId);
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