//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using UnityEngine;

//public enum ControlType { Pitch, Yaw, Roll }

public class ControlSurface : MonoBehaviour
{
	public ControlType controlAxis;
	public int multiplier;

	[Header("Deflection")]

	[Tooltip("Deflection with max positive input."), Range(0, 90)]
	public float max = 15f;

	[Tooltip("Deflection with max negative input"), Range(0, 90)]
	public float min = 15f;

	[Tooltip("Speed of the control surface deflection.")]
	public float moveSpeed = 90f;

	[Tooltip("Requested deflection of the control surface normalized to [-1, 1]. "), Range(-1, 1)]
	public float targetDeflection = 0f;

	[Header("Speed Stiffening")]

	[Tooltip("Wing to use for deflection forces. Deflection limited based on " +
		"airspeed will not function without a reference wing.")]
	[SerializeField] private SimpleWing wing = null;

	[Tooltip("How much force the control surface can exert. The lower this is, " +
		"the more the control surface stiffens with speed.")]
	public float maxTorque = 6000f;

	private Rigidbody rigid = null;
	private Quaternion startLocalRotation = Quaternion.identity;

	private float angle = 0f;

	private bool yawDefined;
	private Vector3 mousePos;

	private void Awake()
	{
		// If the wing has been referenced, then control stiffening will want to be used.
		if (wing != null)
            rigid = GetComponentInParent<Rigidbody>();

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

	private void Start()
	{
		// Dirty hack so that the rotation can be reset before applying the deflection.
		startLocalRotation = transform.localRotation;
		mousePos = Input.mousePosition;
	}

    private void Update()
    {
		Vector3 dmouse = Input.mousePosition - mousePos;
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

    private void FixedUpdate()
	{
		// Different angles depending on positive or negative deflection.
		float targetAngle = targetDeflection > 0f ? targetDeflection * max : targetDeflection * min;

		// How much you can deflect, depends on how much force it would take
		if (rigid != null && wing != null && rigid.velocity.sqrMagnitude > 1f)
		{
			float torqueAtMaxDeflection = rigid.velocity.sqrMagnitude * wing.WingArea;
			float maxAvailableDeflection = Mathf.Asin(maxTorque / torqueAtMaxDeflection) * Mathf.Rad2Deg;

			// Asin(x) where x > 1 or x < -1 is not a number.
			if (float.IsNaN(maxAvailableDeflection) == false)
				targetAngle *= Mathf.Clamp01(maxAvailableDeflection);
		}

		// Move the control surface.
		angle = Mathf.MoveTowards(angle, targetAngle, moveSpeed * Time.fixedDeltaTime);

		// Hacky way to do this!
		transform.localRotation = startLocalRotation;
		transform.Rotate(Vector3.right, angle, Space.Self);
	}

}