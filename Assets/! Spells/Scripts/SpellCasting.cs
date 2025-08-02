using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

public class SpellCasting : MonoBehaviour
{
    private SpellsManager _spellManager;

    [Header("Drawing")]
    public Transform drawPoint;
    public GameObject drawingCanvasObject;
    public Material lineMaterial;
    public float cooldownTime = 1f;
    public float lineWidth = 0.01f;
    public Color lineColor = Color.white;
    public float pointInterval = 0.01f;
    [Range(0, 1)]
    public float matchingThreshold = 0.75f;
    public GameObject aimLine;

    [Header("Matching Algo")]
    [Tooltip("Higher the vectors amount, higher accuracy but more calculation time.")]
    public int vectorAmount = 10;

    [Header("Private Attributes (For Debug Only)")]
    public bool _canDraw = true;
    public bool _isDrawing = false;
    public bool _stopDrawing = false;
    public List<Vector2> _drawnPoints = new List<Vector2>();
    public int _linePositionIndex = 0;
    public Vector3 _newPoint = new Vector3(0, 0, 0);
    public LineRenderer _currentLine;
    public Vector3 _canvaStartingPoint;

    [Header("Private Resamplaing Data (For Debug Only)")]
    public List<Vector2> _resamplePoints = new List<Vector2>();
    public List<Vector2> _resampleVectors = new List<Vector2>();
    public float _totalLength = 0;

    [Header("Testing Assist (For Debug Only)")]
    public List<Vector2> _refResampledVectors = new List<Vector2>();
    public bool debugMode = false;
    public LineRenderer testingLine;
    public TextMeshProUGUI text;

    private void Start()
    {
        // Reset flags
        _canDraw = true;
        _isDrawing = false;
        _stopDrawing = false;

        _spellManager = SpellsManager.instance;

        // Disable the aiming line first
        aimLine.SetActive(false);
    }

    private void Update()
    {
        if (_isDrawing)
        {
            #region Update Drawing
            UpdateDrawing();
            #endregion
        }
        if(_stopDrawing)
        {
            StopDrawing();
        }
    }

    public void UpdateDrawing()
    {
        //_canvaStartingPoint.transform.position = drawPoint.transform.position;

        #region The first point of the drawing
        if (_linePositionIndex == 0)
        {
            // Add new point to the line renderer
            _linePositionIndex++;
            _currentLine.positionCount = _linePositionIndex;
            _newPoint = drawPoint.transform.position - _canvaStartingPoint;

            // Cancel out the z-axis
            _newPoint.z = 0f;
            _currentLine.SetPosition(_linePositionIndex - 1, _newPoint);

            // Add new point to the drawn point list
            _drawnPoints.Add((Vector2)_newPoint);
        }
        #endregion

        #region Follow up drawings
        else
        {
            #region Cancel out the z-axis positions
            _newPoint = drawPoint.transform.position - _canvaStartingPoint;
            _newPoint.z = 0f;
            #endregion

            #region Continue draw if only the draw point has gone a distance from the previous point
            if (Vector3.Distance(_newPoint, _currentLine.GetPosition(_currentLine.positionCount - 1)) > pointInterval)
            {
                // Mana drain per point
                _spellManager.currentMana -= _spellManager.manaDrainPerPoint;

                // Add up the total length along the way for calculation later on
                _totalLength += Vector3.Distance(_newPoint, _currentLine.GetPosition(_currentLine.positionCount - 1));

                // Put points on the line renderer
                _linePositionIndex++;
                _currentLine.positionCount = _linePositionIndex;
                _currentLine.SetPosition(_linePositionIndex - 1, _newPoint);

                // Add new point to the drawn point list for calculation later on
                _drawnPoints.Add((Vector2)_newPoint);
            }
            #endregion
        }
        #endregion
    }

    public void StartDrawing()
    {
        #region Initiate drawing function
        if (_canDraw)
        {
            // Initiate flags and reset values
            _spellManager.isCasting = true;
            _totalLength = 0;
            _canDraw = false;
            _isDrawing = true;

            // Initiate the starting point of canva
            _canvaStartingPoint = drawPoint.transform.position;

            // Instantiate the line renderer
            _currentLine = Instantiate(drawingCanvasObject, _canvaStartingPoint, Quaternion.identity).GetComponent<LineRenderer>();

            #region Line Renderer Customizations (Mat, Color, Width)
            if (lineMaterial != null)
            {
                _currentLine.material = lineMaterial;
            }
            _currentLine.startColor = lineColor;
            _currentLine.startColor = lineColor;
            _currentLine.startWidth = lineWidth;
            _currentLine.endWidth = lineWidth;
            #endregion
        }
        #endregion
    }

    public void StopDrawing()
    {
        // Turn off the flag
        _isDrawing = false;
        _stopDrawing = false;

        if (_drawnPoints.Count >= vectorAmount)
        {

            // Calculation for shape recognition
            ResampleVectors();

            // Reset the drawing
            ResetDrawing();

            #region Debug Mode
            if (debugMode)
            {
                CompareVectors(_refResampledVectors);
            }
            #endregion
            else
            {
                // Find the spell with highest similarity that passed the threshold
                int spellIndex = FindHighestScoreSpell();
                if (spellIndex >= 0)
                {
                    // Starting casting 
                    Debug.Log("Found spell :" + spellIndex);
                    StartCoroutine(StartCasting(spellIndex));
                }
                else
                {
                    // Wand enter cooldown if no spell is being casted
                    CastingCoolDown();
                }
            }
        }
        else
        {
            // Wand enter cooldown if drawn points are too few
            CastingCoolDown();
        }

        Destroy(_currentLine.gameObject);
    }

    public void CastingCoolDown()
    {
        StartCoroutine(DrawingCooldown());
    }

    IEnumerator DrawingCooldown()
    {
        // Turn on the flags first
        _canDraw = false;

        // Start mana regeneration
        _spellManager.ResetManaRegenTimer();
        _spellManager.isCasting = false;

        yield return new WaitForSeconds(cooldownTime);
        // Turn off draw flag after cooldownTime
        _canDraw = true;
    }

    IEnumerator StartCasting(int spellIndex)
    {
        // Enable the aim line
        aimLine.SetActive(true);

        // Charge up the spell
        yield return new WaitForSeconds(_spellManager.equippedSpells[spellIndex].spellData.spellChargeTime);

        // Close the aim line
        aimLine.SetActive(false);

        // Fire the spell
        FireSpell(spellIndex);

        // Wand enter cooldown
        CastingCoolDown();
    }

    public void FireSpell(int spellIndex)
    {
        // Call the spell's shooting function
        DefaultSpellsScript newSpellScript =
            Instantiate(_spellManager.equippedSpells[spellIndex].spellPrefab, drawPoint.position, drawPoint.transform.rotation)
            .GetComponent<DefaultSpellsScript>();
        newSpellScript.ShootProjectile(drawPoint.forward);
    }

    public void ResetDrawing()
    {
        #region Debug Mode
        if (debugMode)
        {
            // Circle Testing
            Vector2 averagePosition = new Vector3(0, 0, 0);
            for (int i = 0; i < _linePositionIndex; i++)
            {
                averagePosition += _drawnPoints[i];
            }
            averagePosition = averagePosition / _linePositionIndex;

            float totalAngle = 0;
            for (int i = 0; i < _linePositionIndex; i++)
            {
                if (i + 1 < _linePositionIndex)
                {
                    totalAngle += Vector2.Angle(_drawnPoints[i] - averagePosition, _drawnPoints[i + 1] - averagePosition);
                }
            }
            Debug.Log("Angle" + totalAngle);

            //testingLine.positionCount = _resampleVectors.Count + 1;
            //for (int i = 0; i < _resampleVectors.Count; i++)
            //{
            //    testingLine.SetPosition(i, _resampleVectors[i]);
            //}
            //testingLine.SetPosition(testingLine.positionCount - 1, averagePosition);

            //testingLine.positionCount = _resamplePoints.Count;
            //for (int i = 0; i < _resamplePoints.Count; i++)
            //{
            //    testingLine.SetPosition(i, _resamplePoints[i]);
            //}
        }
        #endregion

        _linePositionIndex = 0;
        _drawnPoints.Clear();

        //_currentLine = null;

        Destroy(_currentLine);
    }

    public void ResampleVectors()
    {
        #region Clear all the lists if its not empty
        if (_resamplePoints.Count > 0)
            _resamplePoints.Clear();
        if (_resampleVectors.Count > 0)
            _resampleVectors.Clear();
        #endregion

        #region Resample the point list to points of vector amount with average length
        float steps = _totalLength / vectorAmount;
        float currentDistance = 0f;
        int currentIndex = 0;
        _resamplePoints.Add(_drawnPoints[0]);

        for (int i = 1; i < vectorAmount; i++)
        {
            float targetDistance = i * steps;
            while (currentDistance < targetDistance && currentIndex < _drawnPoints.Count - 1)
            {
                float segmentLength = Vector2.Distance(_drawnPoints[currentIndex], _drawnPoints[currentIndex + 1]);
                currentDistance += segmentLength;
                currentIndex++;
            }
            if (currentIndex >= _drawnPoints.Count)
                break;

            float overshootDistance = currentDistance - targetDistance;
            float lerpT = overshootDistance / Vector2.Distance(_drawnPoints[currentIndex - 1], _drawnPoints[currentIndex]);
            Vector2 interpolatedPoint = Vector2.Lerp(_drawnPoints[currentIndex], _drawnPoints[currentIndex - 1], lerpT);
            _resamplePoints.Add(interpolatedPoint);
        }

        _resamplePoints.Add(_drawnPoints[_drawnPoints.Count - 1]);

        for (int i = 0; i < _resamplePoints.Count - 1; i++)
        {
            Vector2 newVector = _resamplePoints[i + 1] - _resamplePoints[i];
            newVector.Normalize();
            _resampleVectors.Add(newVector);
        }
        #endregion
    }

    public float CompareVectors(List<Vector2> targetVectors)
    {
        float matchingScore = 0;
        for (int i = 0; i < vectorAmount; i++)
        {
            matchingScore += Vector2.Dot(_resampleVectors[i], targetVectors[i]);
        }
        #region Show similarity percentage (Debug Mode)
        if (debugMode)
            text.text = (matchingScore / vectorAmount).ToString() + "%";
        #endregion

        Debug.Log("Similarity with ref : " + (matchingScore / vectorAmount));

        return matchingScore / vectorAmount;
    }

    public int FindHighestScoreSpell()
    {
        #region Find the highest maching score (Similarity) from all equipped spells
        float matchingScore = matchingThreshold;
        float subMatchingScore = 0;
        int highestScoreIndex = -1;
        for (int i = 0; i < _spellManager.equippedSpells.Count; i++)
        {
            if (_spellManager.equippedSpells[i] != null)
            {
                //DefaultSpellsScript spellScript = _spellManager.equippedSpells[i].GetComponent<DefaultSpellsScript>();
                //subMatchingScore = CompareVectors(spellScript.spellData.spellsArrayStructure);
                subMatchingScore = CompareVectors(_spellManager.equippedSpells[i].spellData.spellsArrayStructure);
                Debug.Log("Sub:" + subMatchingScore + ":::" + matchingScore);
                if (subMatchingScore >= matchingScore)
                {
                    matchingScore = subMatchingScore;
                    highestScoreIndex = i;
                }
            }
        }
        #endregion

        //Return -1 if no spell is found
        return highestScoreIndex;
    }
}
