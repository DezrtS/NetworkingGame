using UnityEngine;
using UnityEngine.UIElements;

public class Health : MonoBehaviour
{
    [SerializeField] private bool isOwner;
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

    public void TakeDamage(float damage, int damageFrom)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            if (GameManager.Instance.localId == damageFrom) Client.HandleSend($"6<c>{GameManager.Instance.LocalName}", Client.SendType.TCP);
            Die();
        }
    }

    private void Die()
    {
        transform.position = Vector3.zero;
        currentHealth = maxHealth;
    }
}
