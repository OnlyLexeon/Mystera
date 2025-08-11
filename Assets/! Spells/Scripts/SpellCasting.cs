using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellCasting : MonoBehaviour
{
    private SpellsManager _spellManager;

    public GameObject manaBarUI;
    public Slider manaBarValue;

    [Header("Drawing")]
    public Transform drawPoint;
    public Material lineMaterial;
    public float cooldownTime = 1f;
    public float lineWidth = 0.01f;
    public Color lineColor = Color.white;
    public float pointInterval = 0.01f;
    [Range(0, 1)]
    public float matchingThreshold = 0.75f;
    public GameObject aimLine;
    public bool showManaBar = false;

    [Header("Matching Algo")]
    [Tooltip("Higher the vectors amount, higher accuracy but more calculation time.")]
    public int vectorAmount = 10;

    [Header("Private Attributes (Set Public For Debug Only)")]
    public bool _canDraw = true;
    public bool _isDrawing = false;
    public bool _stopDrawing = false;
    public List<Vector2> _drawnPoints = new List<Vector2>();
    public int _linePositionIndex = 0;
    //public Vector3 _newPoint = new Vector3(0, 0, 0);
    public LineRenderer _drawingCanva;
    public DrawingCanvaScript _drawingCanvaAnimation;
    public Transform _canvaStartingPoint;
    public float drawingDeccelerator = 0.2f;

    [Header("Private Resamplaing Data (For Debug Only)")]
    public List<Vector2> _resamplePoints = new List<Vector2>();
    public List<Vector2> _resampleVectors = new List<Vector2>();
    public float _totalLength = 0;

    [Header("Testing Assist (For Debug Only)")]
    public List<Vector2> _refResampledVectors = new List<Vector2>();
    public bool debugMode = false;
    public LineRenderer testingLine;
    public TextMeshProUGUI text;
    private Player _player;

    private void Start()
    {
        // Reset flags
        _canDraw = true;
        _isDrawing = false;
        _stopDrawing = false;

        _spellManager = SpellsManager.instance;
        _player = Player.instance;

        // Disable the aiming line first
        aimLine.SetActive(false);

        showManaBar = false;
        //manaBar.gameObject.SetActive(showManaBar);

        // Initiate the line renderer
        _drawingCanva = _player.drawingCanvas.GetComponent<LineRenderer>();
        _drawingCanvaAnimation = _player.drawingCanvas.GetComponent<DrawingCanvaScript>();
    }

    private void Update()
    {
        if (_isDrawing)
        {
            #region Update Drawing
            UpdateDrawing();
            #endregion
        }
        if (_stopDrawing)
        {
            StopDrawing();
        }
        showManaBar = false;
        if (SpellsManager.instance.manaRegen || _isDrawing)
        {
            showManaBar = true;
        }
        if (showManaBar)
        {
            ShowManaBar();
        }
        if (manaBarUI != null)
            manaBarUI.SetActive(showManaBar);
    }

    public void ShowManaBar()
    {
        if (manaBarValue != null)
            manaBarValue.value = SpellsManager.instance.currentMana / SpellsManager.instance.maxMana;
    }

    public void UpdateDrawing()
    {
        //_canvaStartingPoint.transform.position = drawPoint.transform.position;

        //Vector3 _newPoint = drawPoint.transform.position - _canvaStartingPoint;
        Vector3 _newPoint = (drawPoint.transform.InverseTransformPoint(drawPoint.transform.position) - drawPoint.transform.InverseTransformPoint(_canvaStartingPoint.position)) * drawingDeccelerator;
        //Vector3 _newPoint = _player.transform.InverseTransformPoint(drawPoint.transform.position) - _player.transform.InverseTransformPoint(_canvaStartingPoint.position);
        _newPoint.z = 0f;

        #region The first point of the drawing
        if (_linePositionIndex == 0)
        {
            // Add new point to the line renderer
            _linePositionIndex++;
            _drawingCanva.positionCount = _linePositionIndex;

            _drawingCanva.SetPosition(_linePositionIndex - 1, _newPoint);

            // Add new point to the drawn point list
            _drawnPoints.Add(_newPoint);
        }
        #endregion

        #region Follow up drawings
        else
        {

            #region Continue draw if only the draw point has gone a distance from the previous point
            if (Vector3.Distance(_newPoint, _drawingCanva.GetPosition(_drawingCanva.positionCount - 1)) > pointInterval)
            {
                // Mana drain per point
                if (_spellManager.manaDrain)
                {
                    _spellManager.currentMana -= _spellManager.manaDrainPerPoint;
                    if (_spellManager.currentMana < 0)
                        _spellManager.currentMana = 0;
                }

                // Add up the total length along the way for calculation later on
                _totalLength += Vector3.Distance(_newPoint, _drawingCanva.GetPosition(_drawingCanva.positionCount - 1));

                // Put points on the line renderer
                _linePositionIndex++;
                _drawingCanva.positionCount = _linePositionIndex;
                _drawingCanva.SetPosition(_linePositionIndex - 1, _newPoint);

                // Add new point to the drawn point list for calculation later on
                _drawnPoints.Add(_newPoint);
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
            _drawingCanvaAnimation.OpenDrawing();

            // Initiate the starting point of canva
            //_canvaStartingPoint = drawPoint.transform.position;
            _canvaStartingPoint = new GameObject("CanvaStartingPoint").transform;
            _canvaStartingPoint.position = drawPoint.transform.position;
            _canvaStartingPoint.rotation = drawPoint.transform.rotation;
            _canvaStartingPoint.parent = _player.transform;

            #region Line Renderer Customizations (Mat, Color, Width)
            if (lineMaterial != null)
            {
                _drawingCanva.material = lineMaterial;
            }
            _drawingCanva.startColor = lineColor;
            _drawingCanva.startColor = lineColor;
            _drawingCanva.startWidth = lineWidth;
            _drawingCanva.endWidth = lineWidth;
            #endregion
        }
        #endregion
    }

    public void StopDrawing()
    {
        // Turn off the flag
        _isDrawing = false;
        _stopDrawing = false;
        _drawingCanvaAnimation.CloseDrawing();
        Destroy(_canvaStartingPoint.gameObject);

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
                    //Debug.Log("Found spell :" + spellIndex);
                    if (_spellManager.equippedSpells[spellIndex].spellData.spellManaCost <= _spellManager.currentMana)
                        StartCoroutine(StartCasting(spellIndex));
                    else
                    {
                        // Wand enter cooldown if drawn points are too few
                        CastingCoolDown();
                    }
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
        SpellObject spellObj = _spellManager.equippedSpells[spellIndex];
        // Call the spell's shooting function
        DefaultSpellsScript newSpellScript =
            Instantiate(spellObj.spellPrefab, drawPoint.transform.position, drawPoint.transform.rotation)
            .GetComponent<DefaultSpellsScript>();

        if (spellObj.spellData.spellManaCost <= _spellManager.currentMana)
            _spellManager.currentMana -= spellObj.spellData.spellManaCost;

        newSpellScript.ShootProjectile(drawPoint);
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
        }
        #endregion

        _linePositionIndex = 0;
        _drawnPoints.Clear();
        _drawingCanva.positionCount = 0;
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
        float terbalikMatchingScore = 0;
        for (int i = 0; i < vectorAmount; i++)
        {
            matchingScore += Vector2.Dot(_resampleVectors[i], targetVectors[i]);
            terbalikMatchingScore += Vector2.Dot(_resampleVectors[i], targetVectors[vectorAmount - 1 - i]);
        }
        #region Show similarity percentage (Debug Mode)
        if (debugMode)
            text.text = (matchingScore / vectorAmount).ToString() + "%";
        #endregion

        //Debug.Log("Similarity with ref : " + (matchingScore / vectorAmount));

        //return matchingScore / vectorAmount;
        return matchingScore > Mathf.Abs(terbalikMatchingScore) ? (matchingScore / vectorAmount) : (Mathf.Abs(terbalikMatchingScore) / vectorAmount);
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
                //Debug.Log("Sub:" + subMatchingScore + ":::" + matchingScore);
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
