using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float gravity = 5.0f;

    public float terminalVelocity = 20.0f;

    Vector2 movement = Vector2.zero;

    void Start()
    {
        //setup
    }

    void Update()
    {
        // read inputs
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (!IsGrounded())
        {
            ApplyFall();
        }

        if (movement.magnitude > terminalVelocity)
        {
            movement = movement.normalized * terminalVelocity;
        }

        Debug.Log($"v{movement.magnitude}, mv{terminalVelocity}");

        // GC here but fine for prototype
        transform.position += new Vector3(movement.x, movement.y, 0);
    }

    bool IsGrounded()
    {
        // todo
        return false;
    }

    void ApplyFall()
    {
        movement += Vector2.down * gravity * Time.fixedDeltaTime;
    }
}
