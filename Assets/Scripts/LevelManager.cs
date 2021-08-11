using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private List<Tile> m_tilesList;
    private List<TileCollision> m_playerCollisionList;

    public struct TileCollision
    {
        public Tile tile;
        public float length;
        public Vector2 normal;
    }

    private void Awake()
    {
        m_tilesList = new List<Tile>();
        m_playerCollisionList = new List<TileCollision>();
        GetComponentsInChildren(m_tilesList);
        
        foreach (Tile tile in m_tilesList)
        {
            tile.Subscribe(this);
        }

        Debug.Log($"Tile in level: {m_tilesList.Count}");
    }

    static Color[] cMap = {
        Color.white * 1,
        Color.red * 0.9f,
        Color.green * 0.8f,
        Color.blue * 0.7f,
        Color.cyan * 0.6f,
        Color.magenta * 0.5f,
        Color.yellow * 0.4f,
        Color.red * 0.3f,
        Color.green * 0.2f,
        Color.blue * 0.1f,
    };

    public IList<TileCollision> FindCollidingTiles(PlayerController pc, Vector3 currentPos, Vector3 nextPos)
    {
        m_playerCollisionList.Clear();

        Vector3 toNextPos = nextPos - currentPos;

        foreach (Tile tile in m_tilesList)
        {
            if (pc.CastCheckVsOtherCollider(toNextPos, tile.Collider()))
            {
                Vector3 proj = FindNearestPointOnLine(currentPos, toNextPos.normalized, tile.transform.position);
                float length = (tile.transform.position - proj).sqrMagnitude;

                // Length to projection is less than 0, object is actually behind us
                if (length < 0)
                    continue;

                TileCollision tileCollision = new TileCollision();
                tileCollision.tile = tile;
                tileCollision.length = length;
                tileCollision.normal = CalculateNormal(tile, nextPos);

                m_playerCollisionList.Add(tileCollision);
            }
        }

        m_playerCollisionList.Sort(Comparer<TileCollision>.Create((a, b) => {
            if (a.length > b.length)
                return -1;

            if (a.length < b.length)
                return 1;

            return 0;
        }));

        int tilesColored = 0;
        foreach (TileCollision tileCol in m_playerCollisionList)
        {
            Tile tile = tileCol.tile;
            Vector3 proj = FindNearestPointOnLine(currentPos, toNextPos.normalized, tile.transform.position);

            Debug.DrawLine(tile.transform.position, proj, cMap[tilesColored]);
            tilesColored++;
        }

        return m_playerCollisionList;
    }

    private Vector2 CalculateNormal(Tile tile, Vector2 pos)
    {
        Vector3 posToTileDir = (tile.transform.position - (Vector3)pos).normalized;

        Ray ray = new Ray(pos, posToTileDir);

        RaycastHit2D hit = RaycastForTile(ray, tile);
        
        DebugDrawCross(hit.point, Color.yellow);
        DebugDrawNormal(tile);

        return hit.normal;
    }

    private RaycastHit2D RaycastForTile(Ray ray, Tile tile)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);

        DebugDrawArrow(ray.origin, ray.origin + ray.direction * 2, Color.cyan);

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

    private void DebugDrawCross(Vector2 pos, Color col)
    {
        const float scale = 0.25f;

        Vector3 topLeft = pos + (Vector2.left * scale) + (Vector2.up * scale);
        Vector3 topRight = pos + (Vector2.right * scale) + (Vector2.up * scale);
        Vector3 botLeft = pos + (Vector2.left * scale) + (Vector2.down * scale);
        Vector3 botRight = pos + (Vector2.right * scale) + (Vector2.down * scale);

        Debug.DrawLine(topLeft, botRight, col);
        Debug.DrawLine(topRight, botLeft, col);
    }

    private void DebugDrawNormal(Tile tile)
    {
        Vector2 arrowOrigin = tile.transform.position;
        Vector2 arrowPoint = tile.transform.position + (Vector3)normal * 2;

        DebugDrawArrow(arrowOrigin, arrowPoint, Color.magenta);
    }

    private void DebugDrawArrow(Vector2 arrowOrigin, Vector2 arrowPoint, Color col)
    {
        Vector2 beginToEnd = (arrowPoint - arrowOrigin).normalized;

        Vector2 perp = Vector2.Perpendicular(beginToEnd);
        
        Debug.DrawLine(arrowOrigin, arrowPoint, col);
        Debug.DrawLine(arrowPoint, arrowPoint - (beginToEnd * 0.2f) + (perp * 0.2f), col);
        Debug.DrawLine(arrowPoint, arrowPoint - (beginToEnd * 0.2f) - (perp * 0.2f), col);
    }

    public bool PlayerIsCollidingWithTile(Tile tile)
    {
        foreach (TileCollision colTile in m_playerCollisionList)
        {
            if (tile == colTile.tile)
                return true;
        }

        return false;
    }

    // todo move to other class
    public Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 direction, Vector2 point)
    {
        direction.Normalize();
        Vector2 lhs = point - origin;

        float dotP = Vector2.Dot(lhs, direction);
        return origin + direction * dotP;
    }
}
