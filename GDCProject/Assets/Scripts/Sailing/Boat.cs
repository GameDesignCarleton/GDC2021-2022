using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Water;

public class Boat : MonoBehaviour
{
	Rigidbody rb;
	public Transform mast;

	public float moveForce = 7f;
	public float turnForce = 0.1f;
	public float waveDrag = 1f;

	Quaternion originalMastRotation;
	
	float currentMastRotation;
	[SerializeField]
	float optimumAngle;

	float turning;
	float trim;
	float speed;

	// Start is called before the first frame update
	void Start()
    {
		rb = GetComponent<Rigidbody>();
		originalMastRotation = mast.localRotation;
	}
	void Update()
	{
		speed = Input.GetAxis("Vertical");
		turning = Input.GetAxis("Horizontal");
		trim = Input.GetAxis("Horizontal2");

		currentMastRotation = Mathf.Clamp(currentMastRotation - trim,-90f, 90f);
		mast.localRotation = Quaternion.Slerp(mast.localRotation, originalMastRotation*Quaternion.Euler(0f, 0f, currentMastRotation), 0.1f);

	}


	void FixedUpdate(){
		
		Vector3 relativeVelocity = transform.InverseTransformVector(rb.velocity);
		optimumAngle = Mathf.Sin(Vector3.SignedAngle(transform.forward, Vector3.forward, transform.up)*Mathf.PI/360f)*90f;

		float percentAlignment = 1 - Mathf.Clamp01(Mathf.Abs(currentMastRotation - optimumAngle)/90f);

		rb.AddForce(transform.forward * speed * moveForce * (0.1f + 2 * percentAlignment) * rb.mass);
		//Drag Force
		rb.AddForce(Vector3.ProjectOnPlane(-rb.velocity, Vector3.up) * waveDrag * rb.mass);

		rb.AddRelativeTorque(transform.up * relativeVelocity.z * turning * turnForce);// * rb.mass/1000f);
	}
	

}
