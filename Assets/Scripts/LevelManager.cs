using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public enum Set
    {
        A,
        B,
        STATIC
    }

    private List<Tile> m_tilesListA;
    private List<Tile> m_tilesListB;
    private List<Tile> m_staticTilesList;
    private List<Tile> m_currentTileList;

    private List<TileCollision> m_playerCollisionList;

    [SerializeField]
    private GameObject m_subA;

    [SerializeField]
    private GameObject m_subB;

    [SerializeField]
    private GameObject m_subStatic;

    [SerializeField]
    private Color m_colorA;

    public Color AColor() { return m_colorA; }

    [SerializeField]
    private Color m_colorADisabled;

    [SerializeField]
    private Color m_colorB;

    public Color BColor() { return m_colorB; }

    [SerializeField]
    private Color m_colorBDisabled;

    [SerializeField]
    private Color m_colorStatic;

    public Color StaticColor() { return m_colorStatic; }

    bool m_aSelected = true;

    public bool IsSetA()
    {
        return m_aSelected;
    }

    public struct TileCollision
    {
        public Tile tile;
        public float length;
    }

    private void Awake()
    {
        m_tilesListA = new List<Tile>();
        m_tilesListB = new List<Tile>();
        m_staticTilesList = new List<Tile>();

        m_playerCollisionList = new List<TileCollision>();
        
        m_subA.GetComponentsInChildren(m_tilesListA);
        m_subB.GetComponentsInChildren(m_tilesListB);
        m_subStatic.GetComponentsInChildren(m_staticTilesList);

        foreach (Tile tile in m_tilesListA)
        {
            tile.Subscribe(this, Set.A);
            tile.SetColor(m_colorA, m_colorADisabled);
        }

        foreach (Tile tile in m_tilesListB)
        {
            tile.Subscribe(this, Set.B);
            tile.SetColor(m_colorB, m_colorBDisabled);
        }

        foreach (Tile tile in m_staticTilesList)
        {
            tile.Subscribe(this, Set.STATIC);
            tile.SetColor(m_colorStatic, m_colorStatic);
        }

        m_currentTileList = m_tilesListA;

        Debug.Log($"Tile in level: {m_currentTileList.Count}");
    }

    public RaycastHit2D[] FilterCollisions(RaycastHit2D[] hitsIn)
    {
        List<RaycastHit2D> results = new List<RaycastHit2D>();

        foreach (RaycastHit2D hit in hitsIn)
        {
            if (hit.collider == null)
                continue;

            Tile tile = hit.collider.GetComponentInParent<Tile>();
            if (tile)
            {
                if (m_aSelected && m_tilesListA.Contains(tile))
                {
                    results.Add(hit);
                    continue;
                }

                if (!m_aSelected && m_tilesListB.Contains(tile))
                {
                    results.Add(hit);
                    continue;
                }

                if (m_staticTilesList.Contains(tile))
                {
                    results.Add(hit);
                    continue;
                }
            }
        }

        return results.ToArray();
    }

    public void Flip()
    {
        if (m_aSelected)
        {
            m_currentTileList = m_tilesListB;
        }
        else
        {
            m_currentTileList = m_tilesListA;
        }

        m_aSelected = !m_aSelected;
    }

    public IList<Tile> FindCollidingTiles(PlayerController pc, Vector3 currentPos, Vector3 nextPos)
    {
        m_playerCollisionList.Clear();

        Vector3 velocity = nextPos - currentPos;

        foreach (Tile tile in m_currentTileList)
        {
            CheckCollisionWithTile(tile, pc, velocity, currentPos);
        }

        foreach (Tile tile in m_staticTilesList)
        {
            CheckCollisionWithTile(tile, pc, velocity, currentPos);
        }

        m_playerCollisionList.Sort(Comparer<TileCollision>.Create((a, b) => {
            if (a.length > b.length)
                return -1;

            if (a.length < b.length)
                return 1;

            return 0;
        }));

        for (int i = 0; i < m_playerCollisionList.Count; ++i)
        {
            TileCollision tc = m_playerCollisionList[i];
        }

        List<Tile> result = new List<Tile>();

        foreach (TileCollision col in m_playerCollisionList)
        {
            result.Add(col.tile);
        }

        return result;
    }

    private void CheckCollisionWithTile(Tile tile, PlayerController pc, Vector2 velocity, Vector2 currentPos)
    {
        if (pc.CastCheckVsOtherCollider(velocity, tile.Collider()))
        {
            Vector3 proj = FindNearestPointOnLine(currentPos, velocity.normalized, tile.transform.position);
            float length = (tile.transform.position - proj).sqrMagnitude;

            // Length to projection is less than 0, object is actually behind us
            if (length < 0)
                return;

            TileCollision tileCollision = new TileCollision();
            tileCollision.tile = tile;
            tileCollision.length = length;

            m_playerCollisionList.Add(tileCollision);
        }
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
