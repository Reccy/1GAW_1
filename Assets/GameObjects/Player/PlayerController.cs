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

    public GameObject initVelObj;

    private Vector2 m_velocity;
    private Vector2 m_inputMovement;

    void Start()
    {
        m_velocity = initVelObj.transform.position - transform.position;
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

        if (Input.GetKey(KeyCode.A))
        {
            m_inputMovement += Vector2.left;
        }

        if (Input.GetKey(KeyCode.D))
        {
            m_inputMovement += Vector2.right * 2;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Jump");
            m_inputMovement += Vector2.up * 15;
        }

        m_inputMovement.x = Mathf.Clamp(m_inputMovement.x, -20, 20);
        m_inputMovement.y = Mathf.Clamp(m_inputMovement.y, -20, 20);
    }

    void FixedUpdate()
    {
        // Update velocity with input
        m_velocity += m_inputMovement * Time.fixedDeltaTime;

        // Update velocity with gravity
        m_velocity += Vector2.down * 0.5f * Time.fixedDeltaTime;

        // Clamp speed
        m_velocity.x = Mathf.Clamp(m_velocity.x, -10, 10);
        m_velocity.y = Mathf.Clamp(m_velocity.y, -10, 10);

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
            //Debug.Log($"Could not resolve all collisions on this frame! Remaining: {collidingTiles.Count}");
        }

        //m_level.DebugDrawArrow(transform.position, transform.position + (Vector3)m_velocity, Color.cyan);

        transform.position = transform.position + (Vector3)m_velocity;
    }

    private void LateUpdate()
    {
        Debug.DrawLine(transform.position, GetMouseWorldPos(), Color.yellow);
        Debug.DrawLine(transform.position, transform.position + (Vector3)m_velocity, Color.white);
        //FindObjectOfType<LevelManager>().DebugDrawCross(transform.position, Color.blue);
    }

    List<Tile> DetectCollisions()
    {
        return new List<Tile>(m_level.FindCollidingTiles(this, transform.position, transform.position + (Vector3)m_velocity));
    }

    void ResolveCollisions(List<Tile> tiles)
    {
        //m_velocity = Vector2.zero;

        for (int i = 0; i < tiles.Count; ++i)
        {
            var col = tiles[i];

            ResolveCollision(col);
        }
    }

    void ResolveCollision(Tile tile)
    {
        Vector2 nextPos = transform.position + (Vector3)m_velocity;

        RaycastHit2D[] hits = CastVsOtherCollider(m_velocity, tile.Collider());

        if (hits.Length == 0)
            return;

        RaycastHit2D hit = FindClosestHit(hits, transform.position);

        Vector2 normal = CalculateNormal(tile, hit.point);

        m_level.DebugDrawCross(hit.point, Color.black);

        Bounds bounds = tile.OuterBounds(this);
        float penetration = Mathf.Sqrt(bounds.SqrDistance(hit.point));
        DebugDrawBounds(bounds);

        Vector2 closestPoint = bounds.ClosestPoint(hit.point);
        m_level.DebugDrawCross(closestPoint, Color.red);
        /*
        if (normal == Vector2.left || normal == Vector2.right)
        {
            m_velocity += normal * new Vector2(m_velocity.x, 0);
        }
        else
        {
            m_velocity += normal * new Vector2(0, m_velocity.y);
        }
        */
        /*
        float closestDistance = Mathf.Sqrt(bounds.SqrDistance(nextPos));

        Debug.Log($"{tile.name}: {closestDistance} -> {1 - closestDistance}");

        if (closestDistance < 0)
        {
            Debug.Log($"Closest Distance is too small -> {closestDistance}");
            Debug.DebugBreak();
            return;
        }
        */

        Vector2 deltaV = normal * new Vector2(Mathf.Abs(m_velocity.x), Mathf.Abs(m_velocity.y)) * (1 - penetration);

        //Vector2 deltaV = tile.transform.position + Vector3.Scale(normal, tile.NormalToBounds(this)) - Vector3.Scale(normal, transform.position);

        m_velocity += deltaV;
        /*
        Debug.Log($"normal: {normal}");
        Debug.Log($"dV: {deltaV}");

        m_velocity += deltaV;
        Debug.Log($"fV: {m_velocity}");
        */
    }

    private Vector2 CalculateNormal(Tile tile, Vector2 pos)
    {
        Vector3 posToTileDir = (tile.transform.position - (Vector3)pos).normalized;

        Ray ray = new Ray(pos, posToTileDir);

        RaycastHit2D hit = RaycastForTile(ray, tile);

        m_level.DebugDrawCross(hit.point, Color.yellow);
        DebugDrawNormal(tile, hit.normal);

        return hit.normal;
    }


    private void DebugDrawNormal(Tile tile, Vector2 normal)
    {
        Vector2 arrowOrigin = tile.transform.position;
        Vector2 arrowPoint = tile.transform.position + (Vector3)normal * 2;

        m_level.DebugDrawArrow(arrowOrigin, arrowPoint, Color.magenta);
    }


    private RaycastHit2D RaycastForTile(Ray ray, Tile tile)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);

        m_level.DebugDrawArrow(ray.origin, ray.origin + ray.direction * 2, Color.cyan);

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

    void DebugDrawBounds(Bounds bounds)
    {
        Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y), new Vector3(bounds.min.x, bounds.max.y), Color.blue);
        Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y), new Vector3(bounds.max.x, bounds.max.y), Color.blue);
        Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y), new Vector3(bounds.max.x, bounds.min.y), Color.blue);
        Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y), new Vector3(bounds.min.x, bounds.min.y), Color.blue);
    }

    public float Width()
    {
        return m_collider.bounds.size.x;
    }

    public float Height()
    {
        return m_collider.bounds.size.y;
    }

    Vector2 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        return m_mainCamera.ScreenToWorldPoint(mouse);
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
