using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	CharacterController character;
	Camera mainCamera;
	// Start is called before the first frame update
	void Awake()
    {
		character = GetComponent<CharacterController>();
		mainCamera = Camera.main;
	}

    // Update is called once per frame
    void Update()
    {
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		character.SimpleMove(horizontal * mainCamera.transform.right + vertical * Vector3.Scale(mainCamera.transform.forward, new Vector3(1,0,1)).normalized);
	}
}
