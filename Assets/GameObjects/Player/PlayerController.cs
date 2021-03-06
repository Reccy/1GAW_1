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
    private Collider2D m_groundedCollider;

    [SerializeField]
    private Collider2D m_groundedInputCollider;

    [SerializeField]
    private float m_gravity = 1.0f;

    [SerializeField]
    private float m_maxVerticalSpeed = 2.0f;

    [SerializeField]
    private float m_maxHorizontalSpeed = 2.0f;

    [SerializeField]
    private float m_horizontalDampening = 0.8f;

    private Vector2 m_originalPos;

    private Vector2 m_velocity;
    private Vector2 m_inputMovement;
    private bool m_jumpInput;
    private bool m_isGrounded;
    private bool m_isGroundedInputCheck;

    private void Awake()
    {
        m_originalPos = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
            return;
        }

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

        m_isGroundedInputCheck = ColliderCheck(m_groundedInputCollider);

        if (m_isGroundedInputCheck && Input.GetKeyDown(KeyCode.Space))
        {
            m_jumpInput = true;
        }
    }

    void Reset()
    {
        transform.position = m_originalPos;
        m_velocity = Vector2.zero;
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

        m_isGrounded = ColliderCheck(m_groundedCollider);

        if (m_isGrounded && m_jumpInput)
        {
            m_velocity.y = m_jumpImpulse * Time.fixedDeltaTime;
            m_level.Flip();
            m_jumpInput = false;
        }

        // Clamp speed
        m_velocity.x = Mathf.Clamp(m_velocity.x, -m_maxHorizontalSpeed, m_maxHorizontalSpeed);
        m_velocity.y = Mathf.Clamp(m_velocity.y, -m_maxVerticalSpeed, m_maxVerticalSpeed);

        List<Tile> collidingTiles = DetectCollisions();
        ResolveCollisions(collidingTiles);

        //m_level.DebugDrawArrow(transform.position, transform.position + (Vector3)m_velocity, Color.cyan);

        transform.position = transform.position + (Vector3)m_velocity;
    }

    bool ColliderCheck(Collider2D collider)
    {
        RaycastHit2D[] original = new RaycastHit2D[16];
        collider.Cast(Vector2.zero, original);

        RaycastHit2D[] hits = m_level.FilterCollisions(original);

        return hits.Length > 0;
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
        if (Mathf.Abs(m_velocity.x - 0) < 0.0001f)
        {
            m_velocity.x = 0;
        }

        if (Mathf.Abs(m_velocity.y - 0) < 0.0001f)
        {
            m_velocity.y = 0;
        }

        Vector2 nextPos = (Vector2)transform.position + m_velocity;

        DebugDraw.DrawCross(nextPos, Color.black);
        DebugDraw.DrawArrow(nextPos, tile.transform.position, Color.black);

        RaycastHit2D[] hits = CastVsOtherCollider(m_velocity, tile.Collider());

        if (hits.Length == 0)
            return;

        RaycastHit2D hit = FindClosestHit(hits, nextPos);

        float loc = Vector2.Distance(hit.point, tile.transform.position);
        
        if (loc > 0.7f)
            return;

        Vector2 normal = CalculateNormal(tile, hit.point);

        if (normal == Vector2.right)
        {
            float tCenter = tile.transform.position.x;
            float mCenter = transform.position.x + m_velocity.x;

            float tOverlap = tCenter + (tile.Width() / 2);
            float mOverlap = mCenter - (Width() / 2);

            float overlap = tOverlap - mOverlap;

            if (overlap <= 0)
                return;

            m_velocity.x += overlap;
        }
        else if (normal == Vector2.left)
        {
            float tCenter = tile.transform.position.x;
            float mCenter = transform.position.x + m_velocity.x;

            float tOverlap = tCenter - (tile.Width() / 2);
            float mOverlap = mCenter + (Width() / 2);

            float overlap = mOverlap - tOverlap;

            if (overlap <= 0)
                return;

            m_velocity.x -= overlap;
        }
        else if (normal == Vector2.up)
        {
            float tCenter = tile.transform.position.y;
            float mCenter = transform.position.y + m_velocity.y;

            float tOverlap = tCenter + (tile.Height() / 2);
            float mOverlap = mCenter - (Height() / 2);

            float overlap = tOverlap - mOverlap;

            if (overlap <= 0)
                return;

            m_velocity.y += overlap;
        }
        else if (normal == Vector2.down)
        {
            float tCenter = tile.transform.position.y;
            float mCenter = transform.position.y + m_velocity.y;

            float tOverlap = tCenter - (tile.Height() / 2);
            float mOverlap = mCenter + (Height() / 2);

            float overlap = mOverlap - tOverlap;

            if (overlap <= 0)
                return;

            m_velocity.y -= overlap;
        }
    }

    private Vector2 CalculateNormal(Tile tile, Vector2 pos)
    {
        Vector3 posToTileDir = (tile.transform.position - (Vector3)pos).normalized;

        Ray ray = new Ray(pos, posToTileDir);

        RaycastHit2D hit = RaycastForTile(ray, tile);

        DebugDraw.DrawCross(hit.point, Color.yellow);
        Vector2 normal = hit.normal;

        if (normal != Vector2.up && normal != Vector2.down && normal != Vector2.left && normal != Vector2.right)
        {
            float upCloseness = Vector2.Dot(normal, Vector2.up);
            float leftCloseness = Vector2.Dot(normal, Vector2.left);
            float rightCloseness = Vector2.Dot(normal, Vector2.right);
            float downCloseness = Vector2.Dot(normal, Vector2.down);

            float upDiff = 1 - upCloseness;
            float leftDiff = 1 - leftCloseness;
            float rightDiff = 1 - rightCloseness;
            float downDiff = 1 - downCloseness;

            float select = Mathf.Min(upDiff, leftDiff, rightDiff, downDiff);

            if (select == upDiff)
            {
                normal = Vector2.up;
            }

            if (select == leftDiff)
            {
                normal = Vector2.left;
            }

            if (select == rightDiff)
            {
                normal = Vector2.right;
            }

            if (select == downDiff)
            {
                normal = Vector2.down;
            }
        }

        DebugDrawNormal(tile, normal);

        return normal;
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
#if UNITY_EDITOR
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 150, 80), "Player");

        GUI.Label(new Rect(20, 30, 140, 20), $"Vel: {m_velocity}");
        GUI.Label(new Rect(20, 60, 140, 20), $"Ground: {m_isGrounded}");
    }
#endif
}
