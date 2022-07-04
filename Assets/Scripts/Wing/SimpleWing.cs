//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using UnityEngine;

public enum ControlType { Pitch, Yaw, Roll }

public class SimpleWing : MonoBehaviour
{
	[Tooltip("Size of the wing. The bigger the wing, the more lift it provides.")]
	public Vector2 dimensions = new Vector2(5f, 2f);

	[Tooltip("When true, wing forces will be applied only at the center of mass.")]
	public bool applyForcesToCenter = false;

	[Tooltip("Lift coefficient curve.")]
	public WingCurves wingCurves;
	[Tooltip("The higher the value, the more lift the wing applie at a given angle of attack.")]
	public float liftMultiplier = 1f;
	[Tooltip("The higher the value, the more drag the wing incurs at a given angle of attack.")]
	public float dragMultiplier = 1f;

	public bool isControlSurface;
	public ControlType controlAxis;
	[Tooltip("Set the direction of the control flap."), Range(-1, 1)]
	public int multiplier;

	[Tooltip("Deflection with max positive input."), Range(0, 90)]
	public float max = 15f;

	[Tooltip("Deflection with max negative input."), Range(0, 90)]
	public float min = 15f;

	[Tooltip("Speed of the control surface deflection.")]
	public float moveSpeed = 90f;

	[Tooltip("Hinge about which to rotate.")]
	public Vector3 hinge;

	[Tooltip("Requested deflection of the control surface normalized to [-1, 1]."), Range(-1, 1)]
	public float targetDeflection = 0f;

	[Header("Speed Stiffening")]

	[Tooltip("How much force the control surface can exert. The lower this is, " +
		"the more the control surface stiffens with speed.")]
	public float maxTorque = 6000f;

	private Rigidbody rigid;
	private bool yawDefined;

	private Vector3 liftDirection = Vector3.up;

	private Vector3 forceApplyPos;
	private float liftCoefficient = 0f;
	private float dragCoefficient = 0f;
	private float liftForce = 0f;
	private float dragForce = 0f;
	private float angleOfAttack = 0f;

	private float wingAngle = 0f;
	private Vector3 startPosition;
	private Quaternion startRotation;

	public float AngleOfAttack
	{
		get
		{
			if (rigid != null)
			{
				Vector3 localVelocity = transform.InverseTransformDirection(rigid.velocity);
				return angleOfAttack * -Mathf.Sign(localVelocity.y);
			}
			else
			{
				return 0.0f;
			}
		}
	}

	public float WingArea
	{
		get { return dimensions.x * dimensions.y; }
	}

	public float LiftCoefficient { get { return liftCoefficient; } }
	public float LiftForce { get { return liftForce; } }
	public float DragCoefficient { get { return dragCoefficient; } }
	public float DragForce { get { return dragForce; } }
	public Vector3 ForceApplyPos { get { return forceApplyPos; } }

	public Rigidbody Rigidbody
	{
		set { rigid = value; }
	}

	private void Awake()
	{
		// I don't especially like doing this, but there are many cases where wings might not
		// have the rigidbody on themselves (e.g. they are on a child gameobject of a plane).
		rigid = GetComponentInParent<Rigidbody>();

		if (isControlSurface)
		{
			try
			{
				Input.GetAxis("Yaw");
				yawDefined = true;
			}
			catch (ArgumentException e)
			{
				Debug.LogWarning(e);
				Debug.LogWarning(name + ": \"Yaw\" axis not defined in Input Manager. Rudder will not work correctly!");
			}
		}
	}

	private void Start()
	{
		if (rigid == null)
		{
			Debug.LogError(name + ": SimpleWing has no rigidbody on self or parent!");
		}

		if (wingCurves == null)
		{
			Debug.LogError(name + ": SimpleWing has no defined wing curves!");
		}

		startPosition = transform.localPosition;
		startRotation = transform.localRotation;
	}

	private void Update()
	{
		// Prevent division by zero.
		if (dimensions.x <= 0f)
			dimensions.x = 0.01f;
		if (dimensions.y <= 0f)
			dimensions.y = 0.01f;

		// DEBUG
		if (rigid != null)
		{
			Debug.DrawRay(transform.position, liftDirection * liftForce * 0.0001f, Color.blue);
			Debug.DrawRay(transform.position, -rigid.velocity.normalized * dragForce * 0.0001f, Color.red);
		}

		if (isControlSurface)
        {
			if (controlAxis == ControlType.Pitch)
			{
				targetDeflection = multiplier * Input.GetAxis("Vertical");
				//targetDeflection = Mathf.Clamp(dmouse.y, -1, 1);
			}
			if (controlAxis == ControlType.Roll)
			{
				targetDeflection = multiplier * Input.GetAxis("Horizontal");
			}
			if (controlAxis == ControlType.Yaw && yawDefined)
			{
				targetDeflection = multiplier * Input.GetAxis("Yaw");
				//targetDeflection = Mathf.Clamp(dmouse.x, -1, 1);
			}
        }
	}

    private void FixedUpdate()
    {
		forceApplyPos = (applyForcesToCenter) ? rigid.transform.TransformPoint(rigid.centerOfMass) : transform.position;
		rigid.AddForceAtPosition(CalculateForces(), forceApplyPos, ForceMode.Force);
		if (isControlSurface)
        {
			RotateWing(targetDeflection);
        }
    }

    public Vector3 CalculateForces()
	{
		if (rigid != null && wingCurves != null)
		{
			Vector3 localVelocity = transform.InverseTransformDirection(rigid.GetPointVelocity(transform.position));
			localVelocity.x = 0f;

			// Angle of attack is used as the look up for the lift and drag curves.
			angleOfAttack = Vector3.Angle(Vector3.forward, localVelocity);
			liftCoefficient = wingCurves.GetLiftAtAOA(angleOfAttack);
			dragCoefficient = wingCurves.GetDragAtAOA(angleOfAttack);

			// Calculate lift/drag.
			liftForce = localVelocity.sqrMagnitude * liftCoefficient * WingArea * liftMultiplier;
			dragForce = localVelocity.sqrMagnitude * dragCoefficient * WingArea * dragMultiplier;

			// Vector3.Angle always returns a positive value, so add the sign back in.
			liftForce *= -Mathf.Sign(localVelocity.y);

			// Lift is always perpendicular to air flow.
			liftDirection = Vector3.Cross(rigid.velocity, transform.right).normalized;
			//			rigid.AddForceAtPosition(liftDirection * liftForce, forceApplyPos, ForceMode.Force);
			//
			//			// Drag is always opposite of the velocity.
			//			rigid.AddForceAtPosition(-rigid.velocity.normalized * dragForce, forceApplyPos, ForceMode.Force);

			return liftDirection * liftForce + -rigid.velocity.normalized * dragForce;
		}

		else
			return Vector3.zero;
	}

	public void RotateWing(float targetDeflection)
    {
		targetDeflection = Mathf.Clamp(targetDeflection, -1, 1);
		// Different angles depending on positive or negative deflection.
		float targetAngle = targetDeflection > 0f ? targetDeflection * max : targetDeflection * min;

		// How much you can deflect, depends on how much force it would take
		if (rigid != null && rigid.velocity.sqrMagnitude > 1f)
		{
			float torqueAtMaxDeflection = rigid.velocity.sqrMagnitude * WingArea;
			float maxAvailableDeflection = Mathf.Asin(maxTorque / torqueAtMaxDeflection) * Mathf.Rad2Deg;

			// Asin(x) where x > 1 or x < -1 is not a number.
			if (float.IsNaN(maxAvailableDeflection) == false)
				targetAngle *= Mathf.Clamp01(maxAvailableDeflection);
		}

		// Move the control surface.
		wingAngle = Mathf.LerpAngle(wingAngle, targetAngle, moveSpeed * Time.fixedDeltaTime);
		// Hacky way to do this!
		transform.localPosition = startPosition;
		transform.localRotation = startRotation;
		transform.RotateAround(hinge + transform.position, transform.right, wingAngle);
	}

    // Prevent this code from throwing errors in a built game.
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
	{
		Matrix4x4 oldMatrix = Gizmos.matrix;

		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(dimensions.x, 0f, dimensions.y));
		if (isControlSurface)
        {
			Gizmos.color = Color.red;
			Gizmos.DrawRay(hinge, Vector3.right * multiplier);
        }

		Gizmos.matrix = oldMatrix;
	}
#endif
}