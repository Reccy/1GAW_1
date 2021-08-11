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
            m_inputMovement = -m_velocity;

            if (m_inputMovement.magnitude < 0.01f && m_velocity.magnitude < 0.01f)
            {
                m_inputMovement = Vector2.zero;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            transform.position = mousePos;
        }

        m_inputMovement.x = Mathf.Clamp(m_inputMovement.x, -20, 20);
        m_inputMovement.y = Mathf.Clamp(m_inputMovement.y, -20, 20);

        Debug.DrawLine(transform.position, mousePos, Color.yellow);
    }

    void FixedUpdate()
    {
        // Update velocity with input
        m_velocity += m_inputMovement * Time.fixedDeltaTime;

        // Update velocity with gravity
        m_velocity += Vector2.down * 0.01f * Time.fixedDeltaTime;

        // Clamp speed
        m_velocity.x = Mathf.Clamp(m_velocity.x, -10, 10);
        m_velocity.y = Mathf.Clamp(m_velocity.y, -10, 10);

        List<LevelManager.TileCollision> collidingTiles = new List<LevelManager.TileCollision>();

        int remainingResolutions = 1;
        do
        {
            collidingTiles = DetectCollisions();

            if (collidingTiles.Count > 0)
                ResolveCollisions(collidingTiles);

            remainingResolutions--;
        } while (remainingResolutions > 0 && collidingTiles.Count > 0);

        if (remainingResolutions == 0 && collidingTiles.Count > 0)
        {
            //Debug.Log($"Could not resolve all collisions on this frame! Remaining: {collidingTiles.Count}");
        }

        // Move to new position with velocity
        transform.position = transform.position + (Vector3)m_velocity;
    }

    List<LevelManager.TileCollision> DetectCollisions()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3)m_velocity * 10, Color.red);
        return new List<LevelManager.TileCollision>(m_level.FindCollidingTiles(this, transform.position, transform.position + (Vector3)m_velocity));
    }

    void ResolveCollisions(List<LevelManager.TileCollision> tiles)
    {
        //m_velocity = Vector2.zero;
        foreach (LevelManager.TileCollision tile in tiles)
        {
            ResolveCollision(tile);
        }
    }

    void ResolveCollision(LevelManager.TileCollision col)
    {
        Vector2 nextPos = transform.position + (Vector3)m_velocity;

        Bounds bounds = new Bounds(col.tile.transform.position, new Vector3(col.tile.Width() + Width(), col.tile.Height() + Height()));
        DebugDrawBounds(bounds);

        float closestDistance = Mathf.Sqrt(bounds.SqrDistance(nextPos));

        Debug.Log($"{col.tile.name}: {closestDistance}");

        if (closestDistance < 0 || closestDistance > 0.1)
        {
            Debug.Log($"BREAK: Closest Distance -> {closestDistance}");
            return;
            //Debug.Break();
        }

        m_velocity += col.normal * new Vector2(Mathf.Abs(m_velocity.x), Mathf.Abs(m_velocity.y)) * (1 - closestDistance);
    }

    void DebugDrawBounds(Bounds bounds)
    {
        Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y), new Vector3(bounds.min.x, bounds.max.y), Color.blue);
        Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y), new Vector3(bounds.max.x, bounds.max.y), Color.blue);
        Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y), new Vector3(bounds.max.x, bounds.min.y), Color.blue);
        Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y), new Vector3(bounds.min.x, bounds.min.y), Color.blue);
    }

    float Width()
    {
        return m_collider.bounds.size.x;
    }

    float Height()
    {
        return m_collider.bounds.size.y;
    }

    Vector2 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        return m_mainCamera.ScreenToWorldPoint(mouse);
    }

    public bool CastCheckVsOtherCollider(Vector3 direction, Collider2D other)
    {
        RaycastHit2D[] results = new RaycastHit2D[16];

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

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 150, 40), "Player");

        GUI.Label(new Rect(20, 30, 140, 20), $"Vel: {m_velocity}");
    }
}
