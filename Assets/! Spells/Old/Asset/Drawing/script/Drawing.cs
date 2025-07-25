using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class Drawing : MonoBehaviour
{
    public SlytherinSpellWandScript spellWandScript;

    [SerializeField] private List<GameObject> magic;
    [SerializeField] private InputActionReference triggerAction; //reference to trigger input action
    [SerializeField] private Transform drawPoint; //tip of the weapon where drawing starts
    [SerializeField] private Material lineMaterial;
    private LineRenderer currentLine;
    private bool isDrawing = false;
    private float triggerValue;
    private List<LineRenderer> oldLines = new List<LineRenderer>();

    //cold down
    private bool canDraw = true;
    private float cooldownTime = 1f;
    private float cooldownTimer = 0f;

    //compare reference and user drawing
    [SerializeField] private class ReferenceShape
    {
        public string shapeName;
        public List<Vector2> points; //point of the shape
        //public GameObject magicProjectilePrefab; //corresponding magic effect
    }
    [SerializeField] private float angleMatchThreshold = 15f;
    [SerializeField] private List<ReferenceShape> referenceShapes = new List<ReferenceShape>();
    private List<Vector2> userDrawingPoints;

    //npc animator
    public NPCAnimatorScript npcAnimator;

    private void OnEnable()
    {
        triggerAction.action.Enable();
    }

    private void OnDisable()
    {
        triggerAction.action.Disable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeReferenceShape();
        userDrawingPoints = new List<Vector2>();
    }

    void InitializeReferenceShape()
    {
        //avada kedavra
        ReferenceShape avada = new ReferenceShape
        {
            shapeName = "Avada Kedavra",
            points = new List<Vector2>
            {
                new Vector2(0.5f,0.25f),
                new Vector2(0.25f,0.65f),
                new Vector2(0.75f,0.45f),
                new Vector2(0.5f,1.0f)
            }
            //magicProjectilePrefab = null,
        };
        referenceShapes.Add(avada);

        //crucio
        ReferenceShape stupify = new ReferenceShape
        {
            shapeName = "Stupify",
            points = new List<Vector2>
            {
                new Vector2(0.6f,0.25f),
                new Vector2(0.25f,0.75f),
                new Vector2(0.75f,0.75f)
            }
            //magicProjectilePrefab = null,
        };
        referenceShapes.Add(stupify);

        //imperio
        ReferenceShape crucio = new ReferenceShape
        {
            shapeName = "Crucio",
            points = new List<Vector2>
            {
                new Vector2(0.0f,1.0f),
                new Vector2(0.25f,0.0f),
                new Vector2(0.5f,1.0f),
                new Vector2(0.75f,0.0f),
                new Vector2(1.0f,1.0f)
            }
            //magicProjectilePrefab = null,
        };
        referenceShapes.Add(crucio);

        //stupify
        //ReferenceShape stupify = new ReferenceShape
        //{
        //    shapeName = "Stupify",
        //    points = new List<Vector2>
        //    {
        //        new Vector2(0.6f,0.25f),
        //        new Vector2(0.25f,0.75f),
        //        new Vector2(0.75f,0.75f)
        //    },
        //    magicProjectilePrefab = null,
        //};
        //referenceShapes.Add(stupify);

        if(referenceShapes.Count == 0)
        {
            Debug.Log("No reference shapes defined. Adding a default square.");
            referenceShapes.Add(new ReferenceShape
            {
                shapeName = "DefaultSquare",
                points = new List<Vector2>
                {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(1f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, 0f)
                }
               //magicProjectilePrefab = null,
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!canDraw)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canDraw = true;
                Debug.Log("u now can draw");
            }
        }

        triggerValue = triggerAction.action.ReadValue<float>();
        if (triggerValue > 0.1f && !isDrawing && canDraw)
        {
            StartDrawing();
        } else if (triggerValue <= 0.1f && isDrawing)
        {
            StopDrawing();
        }

        if (isDrawing)
        {
            UpdateDrawing();
        }
    }

    void StartDrawing()
    {
        DestroyOldLines();

        isDrawing = true;
        Debug.Log("canDraw = false");
        canDraw = false;
        userDrawingPoints.Clear(); //clear previous drawing points
        GameObject lineObj = new GameObject("DrawingLine");
        currentLine = lineObj.AddComponent<LineRenderer>();
        oldLines.Add(currentLine);

        //Assign material
        if (lineMaterial != null)
        {
            currentLine.material = lineMaterial;
        }
        else
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lie");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            if (shader != null)
            {
                currentLine.material = new Material(shader);
            }
        }
        currentLine.startColor = Color.white;
        currentLine.endColor = Color.white;
        currentLine.startWidth = 0.01f;
        currentLine.endWidth = 0.01f;
        currentLine.positionCount = 0;
    }

    void UpdateDrawing()
    {
        Vector3 drawPosition = drawPoint.position;
        Debug.Log("position: " + drawPosition);
        currentLine.positionCount++;
        currentLine.SetPosition(currentLine.positionCount - 1, drawPosition);

        Vector3 localPoint = Camera.main.transform.InverseTransformPoint(drawPosition);

        //project the 3d point to 2d
        Vector2 projectedPoint = new Vector2(localPoint.x, localPoint.y);
        userDrawingPoints.Add(projectedPoint);
    }

    void StopDrawing()
    {
        Debug.Log("StopDrawing called");
        isDrawing = false;
        cooldownTimer = cooldownTime;

        ReferenceShape bestMatch = FindBestMatchingShape();
        if (bestMatch != null)
        {
            Debug.Log("Best match: " + bestMatch.shapeName);
            Debug.Log("Shoot magic effect done!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            ShootMagicEffect(bestMatch.shapeName);// bestMatch.magicProjectilePrefab);
        }
        else
        {
            Debug.Log("No matching shape found");
        }

        //destroy the line that draw after 0.8s
        if (currentLine != null)
        {
            Debug.Log("Starting coroutine to destroy line");
            Destroy(currentLine.gameObject, 0.8f);
            currentLine = null;
        }
        else
        {
            Debug.Log("currentLine is null");
        }
    }

    void DestroyOldLines()
    {
        foreach(LineRenderer line in oldLines)
        {
            if(line != null)
            {
                Destroy(line.gameObject);
            }
        }
        oldLines.Clear();
    }

    ReferenceShape FindBestMatchingShape()
    {
        if (userDrawingPoints.Count < 5)
        {
            Debug.Log("Drawing too short to compare");
            return null;
        }

        ReferenceShape bestMatch = null;
        float minAngleDifference = float.MaxValue;

        foreach(ReferenceShape referenceShape in referenceShapes)
        {
            //normalize and resample the user's drawing to mathch thereference drawing's point count
            int targetPointCount = referenceShape.points.Count;
            List<Vector2> normalizedUserPoints = ResamplePoints(userDrawingPoints, targetPointCount);
            List<Vector2> normalizedReferencePoints = ResamplePoints(referenceShape.points, targetPointCount);

            //normalize both set of point to a comman scale
            normalizedUserPoints = NormalizePoints(normalizedUserPoints);
            normalizedReferencePoints = NormalizePoints(normalizedReferencePoints);

            //calculte turning angle for both shape
            List<float> userAngles = CalculateTurningAngles(normalizedUserPoints);
            List<float> referenceAngles = CalculateTurningAngles(normalizedReferencePoints);

            float bestAngleDifferenceForShape = float.MaxValue;
            for(int shitf = 0;shitf<userAngles.Count;shitf++)
            {
                float totalAngleDifferent = 0f;
                for (int i = 0; i < userAngles.Count; i++)
                {
                    int shiftedIndex = (i + shitf) % userAngles.Count;
                    float angleDiff = Mathf.Abs(userAngles[shiftedIndex] - referenceAngles[shiftedIndex]);
                    angleDiff = Mathf.Min(angleDiff, 360f - angleDiff); //handle angle wapping (359 and 1 are close)
                    totalAngleDifferent += angleDiff;
                }
                float averageAngleDifference = totalAngleDifferent / userAngles.Count;
                bestAngleDifferenceForShape = Mathf.Min(bestAngleDifferenceForShape, averageAngleDifference);
            }
            Debug.Log("Average angle diffetence for " + referenceShape.shapeName + " : " + bestAngleDifferenceForShape + " degrees");

            if(bestAngleDifferenceForShape < minAngleDifference)
            {
                minAngleDifference = bestAngleDifferenceForShape;
                bestMatch = referenceShape;
            }
        }

        if (minAngleDifference < angleMatchThreshold)
        {
            return bestMatch;
        }
        return null;
    }

    List<float> CalculateTurningAngles(List<Vector2> points)
    {
        List<float> angles = new List<float>();
        if (points.Count < 3) return angles;

        for(int i = 1;i<points.Count - 1; i++)
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
        if(points.Count >= 3)
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

    List<Vector2> ResamplePoints(List<Vector2> points, int targetCount)
    {
        List<Vector2> resampledPoints = new List<Vector2>();
        float totalLength = 0f;

        //calculate total length of drawing
        for(int i  = 1; i < points.Count; i++)
        {
            totalLength += Vector2.Distance(points[i - 1], points[i]);
        }

        //avoid division by zero
        if (totalLength == 0) return points;

        //calculate the step size for resempleing
        float step = totalLength / (targetCount - 1);
        float currentDisatance = 0f;
        int currentIndex = 0;
        resampledPoints.Add(points[0]);

        for(int i = 1; i < targetCount - 1; i++)
        {
            float targetDistance = i * step;
            while(currentDisatance<targetDistance && currentIndex < points.Count - 1)
            {
                float segmentLength = Vector2.Distance(points[currentIndex], points[currentIndex + 1]);
                currentDisatance += segmentLength;
                currentIndex++;
                
            }
            if (currentIndex >= points.Count) break;

            float overshoot = currentDisatance - targetDistance;
            float t = overshoot / Vector2.Distance(points[currentIndex-1],points[currentIndex]);
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
        for(int i = 1;i<points.Count; i++)
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
        for(int i = 0;i<points.Count; i++)
        {
            float normalizedX = (points[i].x- centerX) / width;
            float normalizedY = (points[i].y - centerY) / height;
            normalizedPoints.Add(new Vector2(normalizedX, normalizedY));
        }
        return normalizedPoints;
    }

    void ShootMagicEffect(string shapeName)
    {
        if(shapeName == "Avada Kedavra")
        {
            spellWandScript.CastSpell(magic[0]);
            npcAnimator.TriggerAnimation("Avada Kedavra");
        }
        else if(shapeName == "Crucio")
        {
            spellWandScript.CastSpell(magic[1]);
            npcAnimator.TriggerAnimation("Crucio");
        }
        else if (shapeName == "Stupify")
        {
            spellWandScript.CastSpell(magic[2]);
            npcAnimator.TriggerAnimation("Stupify");
        }
    }
}
