using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private List<Tile> m_tilesList;
    private List<TileCollision> m_playerCollisionList;

    bool m_pause = false;

    public struct TileCollision
    {
        public Tile tile;
        public float length;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Pause toggle");
            m_pause = !m_pause;
        }
    }

    public IList<Tile> FindCollidingTiles(PlayerController pc, Vector3 currentPos, Vector3 nextPos)
    {
        m_playerCollisionList.Clear();

        Vector3 velocity = nextPos - currentPos;

        foreach (Tile tile in m_tilesList)
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
