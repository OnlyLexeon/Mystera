using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
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
    [Range(0, 100)]
    public float matchingThreshold = 0.75f;

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

    [Header("Private Resamplaing Data")]
    public List<Vector2> resamplePoints = new List<Vector2>();
    public List<Vector2> resampleVectors = new List<Vector2>();
    public float totalLength = 0;
    public float matchingScore = 0f;

    [Header("Testing")]
    public List<Vector2> prevResampledVectors = new List<Vector2>();
    public LineRenderer testingLine;

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
                totalLength += Vector3.Distance(newPoint, currentLine.GetPosition(currentLine.positionCount - 1));
                linePositionIndex++;
                currentLine.positionCount = linePositionIndex;
                currentLine.SetPosition(linePositionIndex - 1, newPoint);
                drawnPoints.Add((Vector2)newPoint);
            }
        }
    }

    public void StartDrawing()
    {
        if (canDraw)
        {
            totalLength = 0;
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
        ResampleVectors();
        ResetDrawing();

        //if (prevResampledVectors.Count > 0)
        //{
        //    CompareVectors(prevResampledVectors);
        //}

        //prevResampledVectors = resampleVectors;

        CompareVectors(prevResampledVectors);

        StartCoroutine(DrawingCooldown());
    }

    IEnumerator DrawingCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        canDraw = true;
    }

    // Shape Recognition Algorithms

    public void ResetDrawing()
    {
        linePositionIndex = 0;
        drawnPoints.Clear();

        //testingLine.positionCount = resamplePoints.Count;
        //for (int i = 0; i < resamplePoints.Count; i++)
        //{
        //    testingLine.SetPosition(i, resamplePoints[i]);
        //}
        testingLine.positionCount = resampleVectors.Count;
        for (int i = 0; i < resampleVectors.Count; i++)
        {
            testingLine.SetPosition(i, resampleVectors[i]);
        }
        //currentLine = null;

        Destroy(currentLine);
    }

    public void ResampleVectors()
    {
        if (resamplePoints.Count > 0)
            resamplePoints.Clear();
        if (resampleVectors.Count > 0)
            resampleVectors.Clear();

        float steps = totalLength / vectorAmount;
        float currentDistance = 0f;
        int currentIndex = 0;
        resamplePoints.Add(drawnPoints[0]);

        for (int i = 1; i < vectorAmount; i++)
        {
            float targetDistance = i * steps;
            while (currentDistance < targetDistance && currentIndex < drawnPoints.Count - 1)
            {
                float segmentLength = Vector2.Distance(drawnPoints[currentIndex], drawnPoints[currentIndex + 1]);
                currentDistance += segmentLength;
                currentIndex++;
            }
            if (currentIndex >= drawnPoints.Count)
                break;

            float overshootDistance = currentDistance - targetDistance;
            float lerpT = overshootDistance / Vector2.Distance(drawnPoints[currentIndex - 1], drawnPoints[currentIndex]);
            Vector2 interpolatedPoint = Vector2.Lerp(drawnPoints[currentIndex], drawnPoints[currentIndex - 1], lerpT);
            resamplePoints.Add(interpolatedPoint);
        }

        resamplePoints.Add(drawnPoints[drawnPoints.Count - 1]);

        for (int i = 0; i < resamplePoints.Count - 1; i++)
        {
            //resampleVectors.Add(resamplePoints[i + 1] - resamplePoints[i]);
            //resampleVectors[i].Normalize();

            Vector2 newVector = resamplePoints[i + 1] - resamplePoints[i];
            newVector.Normalize();
            resampleVectors.Add(newVector);

            //Debug.Log(i +"Sample Vectors :" +resampleVectors[i]);
        }
    }

    public void CompareVectors(List<Vector2> targetVectors)
    {
        matchingScore = 0f;
        for (int i = 0; i < vectorAmount; i++)
        {
            matchingScore += Vector2.Dot(resampleVectors[i], targetVectors[i]);
            Debug.Log(i + ": " + matchingScore);
        }

        Debug.Log("Final Matching Score: " + matchingScore);
    }
}
