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
    private float m_jumpImpulse = 15.0f;

    [SerializeField]
    private Collider2D m_collider;

    [SerializeField]
    private float m_gravity = 1.0f;

    [SerializeField]
    private float m_maxVerticalSpeed = 2.0f;

    [SerializeField]
    private float m_maxHorizontalSpeed = 2.0f;

    [SerializeField]
    private float m_horizontalDampening = 0.8f;

    private Vector2 m_velocity;
    private Vector2 m_inputMovement;
    private bool m_jumpInput;

    void Update()
    {
        float mult = 1.0f;

        if (Input.GetKey(KeyCode.A))
        {
            m_inputMovement.x = -m_speed * mult;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            m_inputMovement.x = m_speed * mult;
        }
        else
        {
            m_inputMovement.x = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_jumpInput = true;
        }
    }

    void FixedUpdate()
    {
        // Apply dampening
        if (m_inputMovement.x == 0)
        {
            m_velocity.x *= m_horizontalDampening * Time.deltaTime;
        }
        else
        {
            // Update velocity with input
            m_velocity.x += m_inputMovement.x * Time.fixedDeltaTime;
        }

        // Update velocity with gravity
        m_velocity += Vector2.down * m_gravity * Time.fixedDeltaTime;

        if (m_jumpInput)
        {
            m_velocity.y = m_jumpImpulse * Time.fixedDeltaTime;
            m_jumpInput = false;
        }

        // Clamp speed
        m_velocity.x = Mathf.Clamp(m_velocity.x, -m_maxHorizontalSpeed, m_maxHorizontalSpeed);
        m_velocity.y = Mathf.Clamp(m_velocity.y, -m_maxVerticalSpeed, m_maxVerticalSpeed);

        List<Tile> collidingTiles = new List<Tile>();

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
            Debug.Log($"Could not resolve all collisions on this frame! Remaining: {collidingTiles.Count}");
        }

        //m_level.DebugDrawArrow(transform.position, transform.position + (Vector3)m_velocity, Color.cyan);

        transform.position = transform.position + (Vector3)m_velocity;
    }

    List<Tile> DetectCollisions()
    {
        return new List<Tile>(m_level.FindCollidingTiles(this, transform.position, transform.position + (Vector3)m_velocity));
    }

    void ResolveCollisions(List<Tile> tiles)
    {
        for (int i = 0; i < tiles.Count; ++i)
        {
            var col = tiles[i];

            ResolveCollision(col);
        }
    }

    void ResolveCollision(Tile tile)
    {
        RaycastHit2D[] hits = CastVsOtherCollider(m_velocity, tile.Collider());

        if (hits.Length == 0)
            return;

        RaycastHit2D hit = FindClosestHit(hits, transform.position);

        Vector2 normal = CalculateNormal(tile, hit.point);

        DebugDraw.DrawCross(hit.point, Color.black);

        Bounds bounds = tile.OuterBounds(this);
        float penetration = Mathf.Sqrt(bounds.SqrDistance(hit.point));
        DebugDraw.DrawBounds(bounds);
        DebugDraw.DrawCross(normal * penetration, Color.blue);

        Vector2 closestPoint = bounds.ClosestPoint(hit.point);
        DebugDraw.DrawCross(closestPoint, Color.red);

        Vector2 deltaV = normal * new Vector2(Mathf.Abs(m_velocity.x), Mathf.Abs(m_velocity.y)) * (1 - penetration);

        m_velocity += deltaV;
    }

    private Vector2 CalculateNormal(Tile tile, Vector2 pos)
    {
        Vector3 posToTileDir = (tile.transform.position - (Vector3)pos).normalized;

        Ray ray = new Ray(pos, posToTileDir);

        RaycastHit2D hit = RaycastForTile(ray, tile);

        DebugDraw.DrawCross(hit.point, Color.yellow);
        DebugDrawNormal(tile, hit.normal);

        return hit.normal;
    }


    private void DebugDrawNormal(Tile tile, Vector2 normal)
    {
        Vector2 arrowOrigin = tile.transform.position;
        Vector2 arrowPoint = tile.transform.position + (Vector3)normal * 2;

        DebugDraw.DrawArrow(arrowOrigin, arrowPoint, Color.magenta);
    }


    private RaycastHit2D RaycastForTile(Ray ray, Tile tile)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);

        DebugDraw.DrawArrow(ray.origin, ray.origin + ray.direction * 2, Color.cyan);

        for (int i = 0; i < hits.Length; ++i)
        {
            RaycastHit2D hit = hits[i];

            if (hit.collider == tile.Collider())
            {
                return hit;
            }
        }

        Debug.LogError("BREAK: hitt has no value");
        Debug.Break();

        // This is invalid!
        return new RaycastHit2D();
    }

    public float Width()
    {
        return m_collider.bounds.size.x;
    }

    public float Height()
    {
        return m_collider.bounds.size.y;
    }

    public bool CastCheckVsOtherCollider(Vector3 velocity, Collider2D other)
    {
        RaycastHit2D[] results = new RaycastHit2D[16];

        int hitCount = m_collider.Cast(velocity.normalized, results, velocity.magnitude, true);

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

    public RaycastHit2D[] CastVsOtherCollider(Vector3 velocity, Collider2D other)
    {
        RaycastHit2D[] results = new RaycastHit2D[16];

        List<RaycastHit2D> realResults = new List<RaycastHit2D>();

        int hitCount = m_collider.Cast(velocity.normalized, results, velocity.magnitude, true);

        int arrBounds = Mathf.Min(results.Length, hitCount);
        for (int i = 0; i < arrBounds; ++i)
        {
            RaycastHit2D hit = results[i];

            if (hit.collider == other)
            {
                realResults.Add(hit);
            }
        }

        return realResults.ToArray();
    }

    public RaycastHit2D FindClosestHit(RaycastHit2D[] hits, Vector3 pos)
    {
        int idx = 0;
        float closest = float.MaxValue;

        for (int i = 0; i < hits.Length; ++i)
        {
            RaycastHit2D hit = hits[i];
            float dist = Vector2.Distance(hit.point, pos);

            if (dist < closest)
            {
                closest = dist;
                idx = i;
            }
        }

        return hits[idx];
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 150, 40), "Player");

        GUI.Label(new Rect(20, 30, 140, 20), $"Vel: {m_velocity}");
    }
}
