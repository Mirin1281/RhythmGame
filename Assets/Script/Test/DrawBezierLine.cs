using UnityEngine;

public class DrawBezierLine : MonoBehaviour
{
    [SerializeField] LineRenderer render;
    [SerializeField] Vector3 start = new Vector2(-5, 0);
    [SerializeField] Vector3 end = new Vector2(5, 0);
    [SerializeField] Vector3 control = new Vector2(2, 3);
    readonly int middlePoints = 10;

    void Start()
    {
        DrawLine(start, end, control);
    }

    void DrawLine(Vector3 start, Vector3 end, Vector3 control)
    {
        int totalPoints = middlePoints + 2;
        render.positionCount = totalPoints;

        var p1 = start + transform.localPosition;
        var p2 = end + transform.localPosition;
        
        render.SetPosition(0, p1);
        for (int i = 1; i < middlePoints + 1; i++)
        {
            float t = (float)i / (totalPoints - 1);
            Vector3 mpos = SampleCurve(p1, p2, (start + end) / 2 + control, t);
            render.SetPosition(i, mpos);
        }
        render.SetPosition(totalPoints - 1, p2);


        static Vector3 SampleCurve(Vector3 start, Vector3 end, Vector3 control, float t)
        {
            Vector3 q0 = Vector3.Lerp(start, control, t);
            Vector3 q1 = Vector3.Lerp(control, end, t);
            Vector3 q2 = Vector3.Lerp(q0, q1, t);
            return q2;
        }
    }
}