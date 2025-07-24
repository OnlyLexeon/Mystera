using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class SpellCasting : MonoBehaviour
{
    [Header("Drawing")]
    public Transform drawPoint;
    public GameObject drawingCanvasObject;
    public Material lineMaterial;
    public float cooldownTime = 1f;
    public float lineWidth = 0.01f;
    public Color lineColor = Color.white;
    public int maxPoints = 100;
    public float pointInterval = 0.01f;
    public bool grabbing = false;

    [Header("Matching Algo")]
    [Tooltip("Higher the vectors amount, higher accuracy but more calculation time.")]
    public int vectorAmount = 10;

    [Header("Private Attributes (For Debug)")]
    public bool canDraw = true;
    public bool isDrawing = false;
    public List<Vector2> drawnPoints = new List<Vector2>();
    public int linePositionIndex = 0;
    public Vector3 newPoint = new Vector3(0, 0, 0);
    public LineRenderer currentLine;
    public Vector3 canvaStartingPoint;

    public List<Vector2> resampleVectors = new List<Vector2>();

    private void Start()
    {
        canDraw = true;
        isDrawing = false;
    }

    private void Update()
    {
        if (isDrawing)
        {
            UpdateDrawing();
        }
    }

    public void UpdateDrawing()
    {
        //canvaStartingPoint.transform.position = drawPoint.transform.position;
        if (linePositionIndex == 0)
        {
            Debug.Log("First Point");
            linePositionIndex++;
            currentLine.positionCount = linePositionIndex;
            newPoint = drawPoint.transform.position - canvaStartingPoint;
            newPoint.z = 0f;
            currentLine.SetPosition(linePositionIndex - 1, newPoint);
            drawnPoints.Add((Vector2)newPoint);
        }
        else
        {
            newPoint = drawPoint.transform.position - canvaStartingPoint;
            newPoint.z = 0f;

            if (Vector3.Distance(newPoint, currentLine.GetPosition(currentLine.positionCount - 1)) > pointInterval)
            {
                linePositionIndex++;
                currentLine.positionCount = linePositionIndex;
                currentLine.SetPosition(linePositionIndex - 1, newPoint);
                drawnPoints.Add((Vector2)newPoint);
            }
        }
    }

    public void ResetDrawing()
    {
        linePositionIndex = 0;
        drawnPoints.Clear();
        Destroy(currentLine);
    }

    public void StartDrawing()
    {
        if (canDraw)
        {
            canDraw = false;
            isDrawing = true;
            canvaStartingPoint = drawPoint.transform.position;
            currentLine = Instantiate(drawingCanvasObject, canvaStartingPoint, Quaternion.identity).GetComponent<LineRenderer>();
            if (lineMaterial != null)
            {
                currentLine.material = lineMaterial;
            }
            currentLine.startColor = lineColor;
            currentLine.startColor = lineColor;
            currentLine.startWidth = lineWidth;
            currentLine.endWidth = lineWidth;
        }
    }

    public void StopDrawing()
    {
        isDrawing = false;
        ResetDrawing();
        StartCoroutine(DrawingCooldown());
    }

    IEnumerator DrawingCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        canDraw = true;
    }

    List<Vector2> ResamplePoints(List<Vector2> points)
    {
        List<Vector2> resampledPoints = new List<Vector2>();
        float totalLength = 0f;

        for(int i =1;i<points.Count;i++)
        {
            totalLength += Vector2.Distance(points[i - 1], points[i]);
        }

        if (totalLength == 0) return points;

        float step = totalLength / (vectorAmount - 1);
        float currentDistance = 0f;
        int currentIndex = 0;
        resampledPoints.Add(points[0]);

        for(int i=1;i<vectorAmount-1;i++)
        {
            float targetDistance = i * step;
            while (currentDistance < targetDistance && currentIndex < points.Count - 1)
            {
                float segmentLength = Vector2.Distance(points[currentIndex], points[currentIndex + 1]);
                currentDistance += segmentLength;
                currentIndex++;

            }
            if (currentIndex >= points.Count) break;

            float overshoot = currentDistance - targetDistance;
            float t = overshoot / Vector2.Distance(points[currentIndex - 1], points[currentIndex]);
            Vector2 interpolatedPoint = Vector2.Lerp(points[currentIndex], points[currentIndex - 1], t);
            resampledPoints.Add(interpolatedPoint);
        }
        resampledPoints.Add(points[points.Count - 1]);
        return resampledPoints;
    }

    List<Vector2> NormalizePoints(List<Vector2> points)
    {
        if (points.Count == 0) return points;

        //find the bounding box
        float minX = points[0].x, maxX = points[0].x;
        float minY = points[0].y, maxY = points[0].y;
        for (int i = 1; i < points.Count; i++)
        {
            minX = Mathf.Min(minX, points[i].x);
            maxX = Mathf.Max(maxX, points[i].x);
            minY = Mathf.Min(minY, points[i].y);
            maxY = Mathf.Min(maxY, points[i].y);
        }

        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;
        float width = maxX - minX;
        float height = maxY - minY;
        if (width == 0 || height == 0) return points; //avoid division by 0

        //normalize to 0-1 range
        List<Vector2> normalizedPoints = new List<Vector2>();
        for (int i = 0; i < points.Count; i++)
        {
            float normalizedX = (points[i].x - centerX) / width;
            float normalizedY = (points[i].y - centerY) / height;
            normalizedPoints.Add(new Vector2(normalizedX, normalizedY));
        }
        return normalizedPoints;
    }

    List<float> CalculateTurningAngles(List<Vector2> points)
    {
        List<float> angles = new List<float>();
        if (points.Count < 3) return angles;

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 p1 = points[i - 1];
            Vector2 p2 = points[i];
            Vector2 p3 = points[i + 1];

            //calculate vector
            Vector2 v1 = p1 - p2;
            Vector2 v2 = p2 - p3;

            float angle = Vector2.Angle(v1, v2);
            angles.Add(angle);
        }

        //connect back to first point (handle tha angle at the last point
        if (points.Count >= 3)
        {
            Vector2 p1 = points[points.Count - 2];
            Vector2 p2 = points[points.Count - 1];
            Vector2 p3 = points[0];
            Vector2 v1 = p1 - p2;
            Vector2 v2 = p3 - p2;
            float angle = Vector2.Angle(v1, v2);
            angles.Add(angle);
        }
        return angles;
    }
}
