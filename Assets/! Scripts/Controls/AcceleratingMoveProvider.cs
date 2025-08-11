using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class AcceleratingMoveProvider : ContinuousMoveProvider
{
    [Header("Acceleration")]
    [Tooltip("Max speed multiplier reached after ramp up.")]
    [SerializeField] float maxSpeedMultiplier = 2.0f;

    [Tooltip("Seconds to ramp from 1x to maxSpeedMultiplier while input is held.")]
    [SerializeField] float timeToMaxSpeed = 1.25f;

    [Tooltip("Stick deadzone below which we consider input 'stopped' and reset speed.")]
    [SerializeField] float inputDeadzone = 0.15f;

    float _speedMul = 1f;

    protected override Vector3 ComputeDesiredMove(Vector2 input)
    {
        // Reset to base instantly when input stops
        if (input.magnitude < inputDeadzone)
        {
            _speedMul = 1f;
            return Vector3.zero;
        }

        // Ramp up while input is held
        float rate = (maxSpeedMultiplier - 1f) / Mathf.Max(0.0001f, timeToMaxSpeed); // per second
        _speedMul = Mathf.MoveTowards(_speedMul, maxSpeedMultiplier, rate * Time.deltaTime);

        var xrOrigin = mediator.xrOrigin;
        if (xrOrigin == null)
            return Vector3.zero;

        // Same movement math as base, but with our speed multiplier applied
        var enableStrafe = this.enableStrafe;
        var inputMove = Vector3.ClampMagnitude(new Vector3(enableStrafe ? input.x : 0f, 0f, input.y), 1f);

        var fwdSrc = forwardSource == null ? xrOrigin.Camera.transform : forwardSource;
        var inputForwardWS = fwdSrc.forward;

        var originTransform = xrOrigin.Origin.transform;
        var speedFactor = moveSpeed * _speedMul * Time.deltaTime * originTransform.localScale.x;

        if (enableFly)
        {
            var inputRightWS = fwdSrc.right;
            var combined = inputMove.x * inputRightWS + inputMove.z * inputForwardWS;
            return combined * speedFactor;
        }

        var originUp = originTransform.up;
        if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardWS, originUp)), 1f))
            inputForwardWS = -fwdSrc.up;

        var inputForwardProjWS = Vector3.ProjectOnPlane(inputForwardWS, originUp);
        var forwardRot = Quaternion.FromToRotation(originTransform.forward, inputForwardProjWS);

        var translationRig = forwardRot * inputMove * speedFactor;
        return originTransform.TransformDirection(translationRig);
    }
}
