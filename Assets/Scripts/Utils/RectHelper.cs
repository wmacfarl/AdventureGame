using UnityEngine;

public class RectHelper
{
    public static void DebugDrawRect(Rect rect, Color color, float duration)
    {
        Vector2[] points = new Vector2[4];
        points[0] = rect.position;
        points[1] = rect.position + Vector2.right * rect.size.x;
        points[2] = rect.position + rect.size;
        points[3] = rect.position + Vector2.up * rect.size.y;

        Debug.DrawLine(points[0], points[1], color, duration);
        Debug.DrawLine(points[1], points[2], color, duration);
        Debug.DrawLine(points[2], points[3], color, duration);
        Debug.DrawLine(points[3], points[0], color, duration);
    }

    static public bool DoRectsTouchInX(Rect rect1, Rect rect2, float epsilon)
    {
        rect1.size += Vector2.right * epsilon;
        rect2.size += Vector2.right * epsilon;
        rect1.position -= Vector2.right * epsilon * .5f;
        rect2.position -= Vector2.right * epsilon * .5f;
        return (rect1.Overlaps(rect2) || rect2.Overlaps(rect1));
    }

    static public bool DoRectsTouchInY(Rect rect1, Rect rect2, float epsilon)
    {
        rect1.size += Vector2.up * epsilon;
        rect2.size += Vector2.up * epsilon;
        rect1.position -= Vector2.up * epsilon * .5f;
        rect2.position -= Vector2.up * epsilon * .5f;
        return (rect1.Overlaps(rect2) || rect2.Overlaps(rect1));
    }

    static public bool DoRectsTouchWithinEpsilon(Rect rect1, Rect rect2, float epsilon)
    {
        return DoRectsTouchInX(rect1, rect2, epsilon) || DoRectsTouchInY(rect1, rect2, epsilon);
    }

    public static Rect RoundToIntegerDimensions(Rect rect)
    {
        rect.position = new Vector2(Mathf.Round(rect.position.x), Mathf.Round(rect.position.y));
        rect.size = new Vector2(Mathf.Round(rect.size.x), Mathf.Round(rect.size.y));
        return rect;
    }

    public static Rect FloorToIntegerDimensions(Rect rect)
    {
        rect.position = new Vector2(Mathf.Floor(rect.position.x), Mathf.Floor(rect.position.y));
        rect.size = new Vector2(Mathf.Floor(rect.size.x), Mathf.Floor(rect.size.y));
        return rect;
    }
}
