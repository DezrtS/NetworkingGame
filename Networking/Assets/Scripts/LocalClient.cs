using UnityEngine;

public class LocalClient : MonoBehaviour
{
    [SerializeField] private bool updatePosition;
    public Vector3 position;

    private int id;
    private GunController gunController;
    private Health health;
    public int Id => id;
    public GunController GunController => gunController;
    public Health Health => health;

    private void Awake()
    {
        gunController = GetComponent<GunController>();
        health = GetComponent<Health>();
    }

    private void Update()
    {
        if (updatePosition) transform.position = position;
    }

    public void SetId(int id)
    {
        this.id = id;
    }

    public void SetPosition(string[] position)
    {
        this.position = new Vector3(float.Parse(position[0]), float.Parse(position[1]), float.Parse(position[2]));
    }
}