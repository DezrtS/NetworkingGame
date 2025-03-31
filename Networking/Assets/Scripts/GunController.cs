using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    private class BulletRequest
    {
        public Vector2 position;
        public Vector2 velocity;

        public BulletRequest(Vector2 position, Vector2 velocity)
        {
            this.position = position;
            this.velocity = velocity;
        }
    }

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireSpeed;
    private ObjectPool bulletObjectPool;

    private List<BulletRequest> bulletRequests = new List<BulletRequest>();

    private void Awake()
    {
        bulletObjectPool = GetComponent<ObjectPool>();
        bulletObjectPool.InitializePool(bulletPrefab);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            FireBulletLocal(transform.position, (mousePosition - (Vector2)transform.position).normalized);
        }

        if (bulletRequests.Count > 0)
        {
            for (int i = 0; i < bulletRequests.Count; i++)
            {
                GameObject bullet = bulletObjectPool.GetObject();
                bullet.GetComponent<Bullet>().OnBulletHit += OnBulletHit;
                bullet.GetComponent<Bullet>().Fire(bulletRequests[i].position, bulletRequests[i].velocity * fireSpeed);
                bulletRequests.RemoveAt(i);
                i--;
            }
        }
    }

    private void OnBulletHit(GameObject bullet)
    {
        bullet.GetComponent<Bullet>().OnBulletHit -= OnBulletHit;
        bulletObjectPool.ReturnToPool(bullet);
    }

    public void FireBulletLocal(Vector2 position, Vector2 direction)
    {
        FireBullet(position, direction);
        int id = GetComponent<LocalClient>().Id;
        Client.HandleSend($"4<c>{id}<id>{position.x},{position.y},{direction.x},{direction.y}", Client.SendType.TCP);
        // Send Message to Server of bullet spawn (position, velocity)
    }

    public void FireBullet(Vector2 position, Vector2 direction)
    {
        bulletRequests.Add(new BulletRequest(position, direction));
    }
}