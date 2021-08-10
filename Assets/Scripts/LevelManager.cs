using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private List<Tile> m_tilesList;
    private List<Tile> m_playerCollisionList;

    private void Awake()
    {
        m_tilesList = new List<Tile>();
        m_playerCollisionList = new List<Tile>();
        GetComponentsInChildren(m_tilesList);
        
        foreach (Tile tile in m_tilesList)
        {
            tile.Subscribe(this);
        }

        Debug.Log($"Tile in level: {m_tilesList.Count}");
    }

    public List<Tile> FindCollidingTiles(PlayerController pc, Vector3 currentPos, Vector3 nextPos)
    {
        m_playerCollisionList.Clear();

        Vector3 toNextPos = nextPos - currentPos;

        foreach (Tile tile in m_tilesList)
        {
            if (pc.RaycastCheckCollision(toNextPos, tile.Collider()))
            {
                m_playerCollisionList.Add(tile);
            }
        }

        return m_playerCollisionList;
    }

    public bool PlayerIsCollidingWithTile(Tile tile)
    {
        foreach (Tile colTile in m_playerCollisionList)
        {
            if (tile == colTile)
                return true;
        }

        return false;
    }
}
