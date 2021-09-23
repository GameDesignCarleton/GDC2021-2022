using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudio : MonoBehaviour {
    private void Update() {


        if (Input.GetKeyUp(KeyCode.G)) {
            AudioManager.instance.PlayAudio(AudioName.ST_001, true);
        }
        if (Input.GetKeyUp(KeyCode.H)) {
            AudioManager.instance.PlayAudio(AudioName.ST_002, true);
        }
        if (Input.GetKeyUp(KeyCode.I)) {
            AudioManager.instance.PlayAudio(AudioName.ST_003, true);
        }


    }
}