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

    public Bounds OuterBounds(PlayerController pc)
    {
        return new Bounds(transform.position, new Vector3(Width() + pc.Width(), Height() + pc.Height()));
    }

    public Vector2 NormalToBounds(PlayerController pc)
    {
        Bounds b = OuterBounds(pc);
        return new Vector2(b.size.x / 2, b.size.y / 2);
    }

    public float Width()
    {
        return m_collider.bounds.size.x;
    }

    public float Height()
    {
        return m_collider.bounds.size.y;
    }
}
