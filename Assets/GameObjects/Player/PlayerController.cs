using UnityEngine;
using System.Collections.Generic;

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Camera m_mainCamera;

    [SerializeField]
    private LevelManager m_level;

    [SerializeField]
    private float m_speed = 1.0f;

    [SerializeField]
    private Collider2D m_collider;
    
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

        List<Tile> collidingTiles = DetectCollisions();

        if (collidingTiles.Count > 0)
        {
            ResolveCollisions(collidingTiles);
        }

        // Move to new position with velocity
        transform.position = transform.position + (Vector3)m_velocity;
    }

    List<Tile> DetectCollisions()
    {
        return m_level.FindCollidingTiles(this, transform.position, transform.position + (Vector3)m_velocity);
    }

    void ResolveCollisions(List<Tile> tiles)
    {
        Debug.Log($"Resolving collisions against {tiles.Count} tiles");
        m_velocity = Vector2.zero;
        return;
    }

    Vector2 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        return m_mainCamera.ScreenToWorldPoint(mouse);
    }

    public bool RaycastCheckCollision(Vector3 direction, Collider2D other)
    {
        RaycastHit2D[] results = new RaycastHit2D[8];

        int hitCount = m_collider.Cast(direction.normalized, results, direction.magnitude, true);

        int arrBounds = Mathf.Min(results.Length, hitCount);
        for (int i = 0; i < arrBounds; ++i)
        {
            RaycastHit2D hit = results[i];

            if (hit.collider == other)
            {
                return true;
            }
        }

        return false;
    }
}
