using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Water;

public class WaterEffects : MonoBehaviour
{
	public MeshRenderer waterMesh;

	// Start is called before the first frame update
	void Awake()
	{
		waterMesh = FindObjectOfType<WaterTile>().GetComponent<MeshRenderer>();
	}

	void Start()
	{
		//For Testing Purposes only
		//CreatePulse(Vector3.zero, 1f);
	}


	// Update is called once per frame
	public void CreatePulse(Vector3 origin, float magnitude)
	{
		waterMesh.sharedMaterial.SetFloat("_PulseStartTime", Time.time);
		waterMesh.sharedMaterial.SetVector("_PulseOrigin", origin);
		waterMesh.sharedMaterial.SetFloat("_PulseAmplitude", magnitude);
	}
}
