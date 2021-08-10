using UnityEngine;

[SelectionBase]
public class Tile : MonoBehaviour
{
    [SerializeField]
    private Collider2D m_collider;

    [SerializeField]
    private SpriteRenderer m_renderer;

    private LevelManager m_levelManager;

    public void Subscribe(LevelManager levelManager)
    {
        m_levelManager = levelManager;
    }

    void Update()
    {
        if (m_levelManager.PlayerIsCollidingWithTile(this))
        {
            m_renderer.color = Color.green;
        }
        else
        {
            m_renderer.color = Color.black;
        }
    }

    public Collider2D Collider()
    {
        return m_collider;
    }
}
