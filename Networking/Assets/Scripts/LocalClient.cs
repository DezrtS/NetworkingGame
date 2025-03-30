using UnityEngine;

public class LocalClient : MonoBehaviour
{
    [SerializeField] private bool updatePosition;
    public Vector3 position;

    private void Update()
    {
        if (updatePosition) transform.position = position;
    }

    public void SetPosition(string[] position)
    {
        this.position = new Vector3(float.Parse(position[0]), float.Parse(position[1]), float.Parse(position[2]));
    }
}