using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private List<Tile> m_tilesList;
    private SortedList<float, Tile> m_playerCollisionList;

    private void Awake()
    {
        m_tilesList = new List<Tile>();
        m_playerCollisionList = new SortedList<float, Tile>();
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

    public IList<Tile> FindCollidingTiles(PlayerController pc, Vector3 currentPos, Vector3 nextPos)
    {
        m_playerCollisionList.Clear();

        Vector3 toNextPos = nextPos - currentPos;

        foreach (Tile tile in m_tilesList)
        {
            if (pc.RaycastCheckCollision(toNextPos, tile.Collider()))
            {
                Vector3 proj = FindNearestPointOnLine(currentPos, toNextPos.normalized, tile.transform.position);
                float length = (tile.transform.position - proj).sqrMagnitude;

                // Length to projection is less than 0, object is actually behind us
                if (length < 0)
                    continue;

                m_playerCollisionList.Add(length, tile);
            }
        }

        int tilesColored = 0;
        foreach (KeyValuePair<float, Tile> kvp in m_playerCollisionList)
        {
            Tile tile = kvp.Value;
            Vector3 proj = FindNearestPointOnLine(currentPos, toNextPos.normalized, tile.transform.position);

            Debug.DrawLine(tile.transform.position, proj, cMap[tilesColored]);
            tilesColored++;
        }

        return m_playerCollisionList.Values;
    }

    public bool PlayerIsCollidingWithTile(Tile tile)
    {
        foreach (Tile colTile in m_playerCollisionList.Values)
        {
            if (tile == colTile)
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
