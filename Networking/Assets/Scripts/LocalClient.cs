using UnityEngine;

public class LocalClient : MonoBehaviour
{
    private bool updatePosition;
    public Vector3 position;

    private int id;
    private GunController gunController;
    private Health health;
    private MovementController movementController;

    public int Id => id;
    public GunController GunController => gunController;
    public Health Health => health;
    public MovementController MovementController => movementController;

    private void Awake()
    {
        gunController = GetComponent<GunController>();
        health = GetComponent<Health>();
        movementController = GetComponent<MovementController>();
    }

    private void Update()
    {
        if (updatePosition)
        {
            updatePosition = false;
            transform.position = position;
        }
    }

    public void SetId(int id)
    {
        this.id = id;
    }

    public void SetPosition(Vector3 position)
    {
        this.position = position;
        updatePosition = true;
    }
}