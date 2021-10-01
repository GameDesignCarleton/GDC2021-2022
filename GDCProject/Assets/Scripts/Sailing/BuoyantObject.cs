using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Water;

public class BuoyantObject : MonoBehaviour
{
    Rigidbody rb;
	Material matInstance;

	public float centreOfPressure = 3.1f;
	public float buoyantForce = 1f;
	public float waveEffect = 0.5f;
	public float damping = 5f;
	public float tiltFactor = 5f;
	public float angularDamping = 1f;

	Vector4 _GAmplitude;
	Vector4 _GFrequency;
	Vector4 _GSteepness;
	Vector4 _GSpeed;
	Vector4 _GDirectionAB;
	Vector4 _GDirectionCD;
	float _GerstnerIntensity;
	float _Time;

	float _PulseAmplitude;
	Vector3 _PulseOrigin;
	float _PulseStartTime;
	
	

	// Start is called before the first frame update
	void Start()
    {
		rb = GetComponent<Rigidbody>();
		matInstance = FindObjectOfType<WaterTile>().GetComponent<MeshRenderer>().sharedMaterial;

		_GAmplitude = matInstance.GetVector("_GAmplitude");
		_GFrequency = matInstance.GetVector("_GFrequency");
		_GSteepness = matInstance.GetVector("_GSteepness");
		_GSpeed = matInstance.GetVector("_GSpeed");
		_GDirectionAB = matInstance.GetVector("_GDirectionAB");
		_GDirectionCD = matInstance.GetVector("_GDirectionCD");
		_GerstnerIntensity = matInstance.GetFloat("_GerstnerIntensity");
	}


	void FixedUpdate(){
		_PulseAmplitude = matInstance.GetFloat("_PulseAmplitude");
		_PulseOrigin = matInstance.GetVector("_PulseOrigin");
		_PulseStartTime = matInstance.GetFloat("_PulseStartTime");

		_Time = Shader.GetGlobalVector("_Time").y;
		CalculateBuoyantForce();
		CalculateLevelingTorque();
	}
	
	void CalculateBuoyantForce()
	{
		float gerstnerHeight = GerstnerOffset4(
			new Vector2(rb.transform.position.x, rb.transform.position.z),
			_GSteepness,                                                // steepness
			_GAmplitude,                                                // amplitude
			_GFrequency,                                                // frequency
			_GSpeed,                                                    // speed
			_GDirectionAB,                                              // direction # 1, 2
			_GDirectionCD                                               // direction # 3, 4
		).y;

		float pulseHeight = PulseStrength(new Vector2(rb.transform.position.x, rb.transform.position.z));
		float gerstnerDelta = gerstnerHeight * waveEffect + centreOfPressure + pulseHeight - transform.position.y;
		
		if (gerstnerDelta > 0)
		{
			rb.AddForce(Mathf.Max(0f, gerstnerDelta) * Vector3.up * buoyantForce * rb.mass * 9.81f - Vector3.up * rb.velocity.y * rb.mass * damping);
		}
	}

	void CalculateLevelingTorque(){
		Vector3 normal = GerstnerNormal4 (
			new Vector2(rb.transform.position.x,rb.transform.position.z),
			_GAmplitude,												// amplitude
			_GFrequency,												// frequency
			_GSpeed,													// speed
			_GDirectionAB,												// direction # 1, 2
			_GDirectionCD												// direction # 3, 4
		);
		rb.AddTorque(Vector3.Cross(normal - transform.up, transform.up) * -tiltFactor);
		rb.AddTorque(-rb.angularVelocity * angularDamping);
	}

	float TimedAmplitude(){
		float x = (_Time-_PulseStartTime);
		return 1/(1+Mathf.Exp(-1*(x-3)));
	}

	float PulseStrength(Vector3 worldSpaceVertex){
		float wavelength = 0.1f;
		float waveSpeed = 10;
		float pulseDistance = Vector3.Distance(worldSpaceVertex,_PulseOrigin);
		float wavePosition = (pulseDistance-waveSpeed*(_Time-_PulseStartTime));
		
		return 250*_PulseAmplitude*TimedAmplitude()*/*exp(-(_Time.y-_PulseStartTime)*0.3f)*/Mathf.Sin(wavelength * wavePosition)/(wavePosition);		
	}

	Vector3 GerstnerOffset4 (Vector2 xzVtx, Vector4 steepness, Vector4 amp, Vector4 freq, Vector4 speed, Vector4 dirAB, Vector4 dirCD) 
	{
		Vector3 offsets;
		
		Vector4 AB = new Vector4(
			freq.x * amp.x * dirAB.x,
			freq.x * amp.x * dirAB.y,
			freq.y * amp.y * dirAB.z,
			freq.y * amp.y * dirAB.w
		);
		Vector4 CD = new Vector4(
			freq.z * amp.z * dirCD.x,
			freq.z * amp.z * dirCD.y,
			freq.w * amp.w * dirCD.z,
			freq.w * amp.w * dirCD.w
		);
		
		Vector4 dotABCD = Vector4.Scale(freq, new Vector4(
			Vector2.Dot(new Vector2(dirAB.x, dirAB.y), xzVtx), 
			Vector2.Dot(new Vector2(dirAB.z, dirAB.w), xzVtx), 
			Vector2.Dot(new Vector2(dirCD.x, dirCD.y), xzVtx), 
			Vector2.Dot(new Vector2(dirCD.z, dirCD.w), xzVtx)
		));

		Vector4 TIME = Vector4.Scale(new Vector4(_Time,_Time,_Time,_Time), speed);

		Vector4 COS = new Vector4(
			Mathf.Cos(dotABCD.x + TIME.x),
			Mathf.Cos(dotABCD.y + TIME.y),
			Mathf.Cos(dotABCD.z + TIME.z),
			Mathf.Cos(dotABCD.w + TIME.w)
		);

		Vector4 SIN = new Vector4(
			Mathf.Sin(dotABCD.x + TIME.x),
			Mathf.Sin(dotABCD.y + TIME.y),
			Mathf.Sin(dotABCD.z + TIME.z),
			Mathf.Sin(dotABCD.w + TIME.w)
		);
		
		offsets.x = Vector4.Dot(COS, new Vector4(AB.x, AB.z, CD.x, CD.z));
		offsets.z = Vector4.Dot(COS, new Vector4(AB.y, AB.w, CD.y, CD.w));
		offsets.y = Vector4.Dot(SIN, amp);

		return offsets;			
	}	

	Vector3 GerstnerNormal4 (Vector2 xzVtx, Vector4 amp, Vector4 freq, Vector4 speed, Vector4 dirAB, Vector4 dirCD) 
	{
		Vector3 nrml = new Vector3(0,2.0f,0);

		Vector4 AB = new Vector4(
			freq.x * amp.x * dirAB.x,
			freq.x * amp.x * dirAB.y,
			freq.y * amp.y * dirAB.z,
			freq.y * amp.y * dirAB.w
		);
		Vector4 CD = new Vector4(
			freq.z * amp.z * dirCD.x,
			freq.z * amp.z * dirCD.y,
			freq.w * amp.w * dirCD.z,
			freq.w * amp.w * dirCD.w
		);
		
		Vector4 dotABCD = Vector4.Scale(freq, new Vector4(
			Vector2.Dot(new Vector2(dirAB.x, dirAB.y), xzVtx), 
			Vector2.Dot(new Vector2(dirAB.z, dirAB.w), xzVtx), 
			Vector2.Dot(new Vector2(dirCD.x, dirCD.y), xzVtx), 
			Vector2.Dot(new Vector2(dirCD.z, dirCD.w), xzVtx)
		));

		Vector4 COS = new Vector4(
			Mathf.Cos(dotABCD.x + _Time),
			Mathf.Cos(dotABCD.y + _Time),
			Mathf.Cos(dotABCD.z + _Time),
			Mathf.Cos(dotABCD.w + _Time)
		);

		nrml.x -= Vector4.Dot(COS, new Vector4(AB.x, AB.z, CD.x, CD.z));
		nrml.z -= Vector4.Dot(COS, new Vector4(AB.y, AB.w, CD.y, CD.w));
		
		nrml.x *= _GerstnerIntensity;
		nrml.z *= _GerstnerIntensity;

		nrml = Vector4.Normalize (nrml);

		return nrml;			
	}

}
