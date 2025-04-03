 using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private bool isOwner;
    [SerializeField] private float acceleration;
    [SerializeField] private float accelerationTime;
    [SerializeField] private float maxSpeed;

    [SerializeField] private float sendDelay = 0;
    private float sendTimer = 0;

    private Rigidbody2D rig;
    private Vector2 moveInput = Vector2.zero;
    private Vector2 velocity = Vector2.zero;
    private bool updateVelocity;

    private void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (updateVelocity)
        {
            rig.linearVelocity = velocity;
            updateVelocity = false;
        }
        if (isOwner)
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                sendDelay -= 0.1f;
                Debug.Log("Send delay: " + sendDelay);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                sendDelay += 0.1f;
                Debug.Log("Send delay: " + sendDelay);
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 velocityChange = HandleMovement(moveInput, maxSpeed, acceleration, accelerationTime, accelerationTime, rig.linearVelocity);
        ApplyForce(velocityChange, ForceMode2D.Force);

        if (!isOwner) return;
        if (Client.end || !Client.isConnected || !Client.hasId) return;
        sendTimer -= Time.fixedDeltaTime;
        if (sendTimer <= 0)
        {
            Vector3 position = transform.position;
            Client.HandleSend($"0<c>{GameManager.Instance.localId}<id>{position.x},{position.y},{position.z},{rig.linearVelocityX},{rig.linearVelocityY},{moveInput.x},{moveInput.y}", Client.SendType.UDP);
            sendTimer = sendDelay;
        }
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        this.moveInput = moveInput;
    }

    public void SetVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
        updateVelocity = true;
    }

    public void ApplyForce(Vector2 force, ForceMode2D forceMode)
    {
        rig.AddForce(force, forceMode);
    }

    protected static float GetAcceleration(float maxSpeed, float timeToReachFullSpeed)
    {
        if (timeToReachFullSpeed == 0)
        {
            return maxSpeed;
        }

        return (maxSpeed) / timeToReachFullSpeed;
    }

    protected static Vector3 HandleMovement(Vector3 move, float maxSpeed, float acceleration, float timeToAccelerate, float timeToDeaccelerate, Vector3 currentVelocity)
    {
        Vector3 targetVelocity = move.normalized * maxSpeed;
        float targetSpeed = targetVelocity.magnitude;

        Vector3 velocityDifference = targetVelocity - currentVelocity;
        Vector3 differenceDirection = velocityDifference.normalized;
        float accelerationIncrement;

        if (currentVelocity.magnitude <= targetSpeed)
        {
            accelerationIncrement = GetAcceleration(acceleration, timeToAccelerate) * Time.deltaTime;
        }
        else
        {
            accelerationIncrement = GetAcceleration(acceleration, timeToDeaccelerate) * Time.deltaTime;
        }

        if (velocityDifference.magnitude < accelerationIncrement)
        {
            return velocityDifference;
        }
        else
        {
            return differenceDirection * accelerationIncrement;
        }
    }
}