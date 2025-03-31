 using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private bool swapYZ;
    [SerializeField] private float speed = 15;

    void Update()
    {
        float deltaTime = Time.deltaTime;
        if (swapYZ)
            transform.Translate(Input.GetAxis("Horizontal") * deltaTime * speed, Input.GetAxis("Vertical") * deltaTime * speed, 0);
        else
            transform.Translate(Input.GetAxis("Horizontal") * deltaTime * speed, 0, Input.GetAxis("Vertical") * deltaTime * speed);
    }
}