 using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private float speed = 15;

    void Update()
    {
        float deltaTime = Time.deltaTime;
        transform.Translate(Input.GetAxis("Horizontal") * deltaTime * speed, 0, Input.GetAxis("Vertical") * deltaTime * speed);
    }
}