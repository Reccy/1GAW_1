using UnityEngine;

[SelectionBase]
public class Tile : MonoBehaviour
{
    [SerializeField]
    private Collider2D m_collider;

    [SerializeField]
    private SpriteRenderer m_renderer;

    private Color m_color;
    private Color m_disabledColor;

    private LevelManager m_levelManager;
    private LevelManager.Set m_set;

    private void Update()
    {
        if (m_set == LevelManager.Set.A && m_levelManager.IsSetA())
        {
            m_renderer.color = m_color;
        }
        else if (m_set == LevelManager.Set.B && !m_levelManager.IsSetA())
        {
            m_renderer.color = m_color;
        }
        else
        {
            m_renderer.color = m_disabledColor;
        }
    }

    public void Subscribe(LevelManager levelManager, LevelManager.Set set)
    {
        m_levelManager = levelManager;
        m_set = set;
    }

    public void SetColor(Color color, Color disabled)
    {
        m_color = color;
        m_disabledColor = disabled;
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
