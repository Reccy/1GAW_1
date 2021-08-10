using UnityEngine;

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Camera m_mainCamera;
    
    [SerializeField]
    private float m_speed = 1.0f;
    
    private Vector2 m_velocity;
    private Vector2 m_inputMovement;

    void Start()
    {
        //setup
    }

    void Update()
    {
        Vector2 mousePos = GetMouseWorldPos();

        if (Input.GetMouseButton(0))
        {
            m_inputMovement = ((Vector3)mousePos - transform.position) * m_speed;
        }
        else
        {
            m_inputMovement = Vector2.zero;
        }

        Debug.DrawLine(transform.position, mousePos, Color.green);
    }

    void FixedUpdate()
    {
        // Update velocity with input
        m_velocity += m_inputMovement * Time.fixedDeltaTime;

        // Clamp speed
        m_velocity.x = Mathf.Clamp(m_velocity.x, -10, 10);
        m_velocity.y = Mathf.Clamp(m_velocity.y, -10, 10);

        // Move to new position with velocity
        transform.position = transform.position + (Vector3)m_velocity;
    }

    Vector2 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        return m_mainCamera.ScreenToWorldPoint(mouse);
    }
}
