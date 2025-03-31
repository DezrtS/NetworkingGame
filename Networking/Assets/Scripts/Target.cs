using UnityEngine;

public class Target : MonoBehaviour
{
    private int id;
    public void InitializeTarget(int id)
    {
        this.id = id;
    }

    public void HitTarget()
    {
        // Send Message to server to notify of potential hit
    }
}
