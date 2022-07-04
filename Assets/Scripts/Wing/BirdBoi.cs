//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;

public class BirdBoi : MonoBehaviour
{
	public GameObject rightWing;
	public GameObject rightArelion;
	public GameObject leftWing;
	public GameObject leftArelion;

	private SimpleWing leftWingScript;
	private SimpleWing leftArelionScript;
	private SimpleWing rightWingScript;
	private SimpleWing rightArelionScript;
	public float angle = 40;
	public Rigidbody rb { get; internal set; }

	private bool dive;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		leftWingScript = leftWing.GetComponent<SimpleWing>();
		leftArelionScript = leftArelion.GetComponent<SimpleWing>();
		rightWingScript = rightWing.GetComponent<SimpleWing>();
		rightArelionScript = rightArelion.GetComponent<SimpleWing>();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.LeftShift))
		{
			leftWingScript.enabled = false;
			leftArelionScript.enabled = false;
			rightWingScript.enabled = false;
			rightArelionScript.enabled = false;
		}
		else
		{
			leftWingScript.enabled = true;
			leftArelionScript.enabled = true;
			rightWingScript.enabled = true;
			rightArelionScript.enabled = true;
		}
	}

    private void FixedUpdate()
    {

    }

    private float CalculatePitchG()
	{
		// Angular velocity is in radians per second.
		Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
		Vector3 localAngularVel = transform.InverseTransformDirection(rb.angularVelocity);

		// Local pitch velocity (X) is positive when pitching down.

		// Radius of turn = velocity / angular velocity
		float radius = (Mathf.Approximately(localAngularVel.x, 0.0f)) ? float.MaxValue : localVelocity.z / localAngularVel.x;

		// The radius of the turn will be negative when in a pitching down turn.

		// Force is mass * radius * angular velocity^2
		float verticalForce = (Mathf.Approximately(radius, 0.0f)) ? 0.0f : (localVelocity.z * localVelocity.z) / radius;

		// Express in G (Always relative to Earth G)
		float verticalG = verticalForce / -9.81f;

		// Add the planet's gravity in. When the up is facing directly up, then the full
		// force of gravity will be felt in the vertical.
		verticalG += transform.up.y * (Physics.gravity.y / -9.81f);

		return verticalG;
	}

	private void OnGUI()
	{
		const float msToKnots = 1.94384f;
		GUI.Label(new Rect(10, 40, 300, 20), string.Format("Speed: {0:0.0} knots", rb.velocity.magnitude * msToKnots));
		//GUI.Label(new Rect(10, 60, 300, 20), string.Format("Throttle: {0:0.0}%", throttle * 100.0f));
		GUI.Label(new Rect(10, 60, 300, 20), string.Format("G Load: {0:0.0} G", CalculatePitchG()));
		//GUI.Label(new Rect(10, 80, 300, 20), string.Format("Angle Of Attack: "))
	}
}
