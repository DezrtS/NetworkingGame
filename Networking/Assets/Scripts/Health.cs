using UnityEngine;
using UnityEngine.UIElements;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetHealth(float health)
    {
        currentHealth = health; 
    }

    public void TakeDamageLocal(float damage)
    {
        float previousHealth = currentHealth;
        TakeDamage(damage);
        int id = GetComponent<LocalClient>().Id;
        Client.HandleSend($"5<c>{id}<id>{damage},{previousHealth}", Client.SendType.TCP);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        transform.position = Vector3.zero;
        currentHealth = maxHealth;
    }
}
