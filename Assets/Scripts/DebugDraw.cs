using UnityEngine;

public class DebugDraw : MonoBehaviour
{
    public static void DrawCross(Vector2 pos, Color col)
    {
        const float scale = 0.25f;

        Vector3 topLeft = pos + (Vector2.left * scale) + (Vector2.up * scale);
        Vector3 topRight = pos + (Vector2.right * scale) + (Vector2.up * scale);
        Vector3 botLeft = pos + (Vector2.left * scale) + (Vector2.down * scale);
        Vector3 botRight = pos + (Vector2.right * scale) + (Vector2.down * scale);

        Debug.DrawLine(topLeft, botRight, col);
        Debug.DrawLine(topRight, botLeft, col);
    }

    public static void DrawArrow(Vector2 arrowOrigin, Vector2 arrowPoint, Color col)
    {
        Vector2 beginToEnd = (arrowPoint - arrowOrigin).normalized;

        Vector2 perp = Vector2.Perpendicular(beginToEnd);

        Debug.DrawLine(arrowOrigin, arrowPoint, col);
        Debug.DrawLine(arrowPoint, arrowPoint - (beginToEnd * 0.2f) + (perp * 0.2f), col);
        Debug.DrawLine(arrowPoint, arrowPoint - (beginToEnd * 0.2f) - (perp * 0.2f), col);
    }

    public static void DrawBounds(Bounds bounds)
    {
        Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y), new Vector3(bounds.min.x, bounds.max.y), Color.blue);
        Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y), new Vector3(bounds.max.x, bounds.max.y), Color.blue);
        Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y), new Vector3(bounds.max.x, bounds.min.y), Color.blue);
        Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y), new Vector3(bounds.min.x, bounds.min.y), Color.blue);
    }
}
