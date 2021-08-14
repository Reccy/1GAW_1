using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private List<Tile> m_tilesListA;
    private List<Tile> m_tilesListB;
    private List<Tile> m_currentTileList;

    private List<TileCollision> m_playerCollisionList;

    [SerializeField]
    private GameObject m_subA;

    [SerializeField]
    private GameObject m_subB;

    [SerializeField]
    private Color m_colorA;

    [SerializeField]
    private Color m_colorADisabled;

    [SerializeField]
    private Color m_colorB;

    [SerializeField]
    private Color m_colorBDisabled;

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

        m_playerCollisionList = new List<TileCollision>();
        
        m_subA.GetComponentsInChildren(m_tilesListA);
        m_subB.GetComponentsInChildren(m_tilesListB);

        foreach (Tile tile in m_tilesListA)
        {
            tile.Subscribe(this, true);
            tile.SetColor(m_colorA, m_colorADisabled);
        }

        foreach (Tile tile in m_tilesListB)
        {
            tile.Subscribe(this, false);
            tile.SetColor(m_colorB, m_colorBDisabled);
        }

        m_currentTileList = m_tilesListA;

        Debug.Log($"Tile in level: {m_currentTileList.Count}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Flip();
        }
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
            Vector2 hit = Vector2.zero;

            if (pc.CastCheckVsOtherCollider(velocity, tile.Collider()))
            {
                Vector3 proj = FindNearestPointOnLine(currentPos, velocity.normalized, tile.transform.position);
                float length = (tile.transform.position - proj).sqrMagnitude;

                // Length to projection is less than 0, object is actually behind us
                if (length < 0)
                    continue;

                TileCollision tileCollision = new TileCollision();
                tileCollision.tile = tile;
                tileCollision.length = length;

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

        List<Tile> result = new List<Tile>();

        foreach (TileCollision col in m_playerCollisionList)
        {
            result.Add(col.tile);
        }

        return result;
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
