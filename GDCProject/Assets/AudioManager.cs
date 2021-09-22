using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;
    [SerializeField] private Debugging debug;
    [SerializeField] private AudioTrack[] tracks;

    private int masterVolume = 5;
    private bool masterMute;
    
    private Hashtable audioTable;
    private Hashtable jobTable;

    [System.Serializable]
    public class AudioObject {
        public string nickname;
        public AudioName name;
        public AudioClip intro;
        public AudioClip clip;
    }

    [System.Serializable]
    public class AudioTrack {
        public string trackName;
        public bool soundtracks;

        [HideInInspector] public int volume = 5;
        [HideInInspector] public bool mute;
        [HideInInspector] public AudioSource source;
        [HideInInspector] public AudioSource secondSource;
        [HideInInspector] public bool secondSourceActive;

        public AudioObject[] audio;

        public IEnumerator currentVolumeCoroutineSource;
        public IEnumerator currentVolumeCoroutineSecondSource;
    }

    public void Start() {
        foreach (AudioTrack _track in tracks) {
            GameObject childObject = new GameObject(_track.trackName);
            childObject.transform.parent = this.transform;
            _track.source = childObject.AddComponent<AudioSource>();

            if (_track.soundtracks) {
                GameObject childObject2 = new GameObject($"{_track.trackName} 2");
                childObject2.transform.parent = this.transform;
                _track.secondSource = childObject2.AddComponent<AudioSource>();
            }
        }
    }

    [System.Serializable]
    public class Debugging {
        public bool log;
        public bool inspector;
        public bool nothing;
    }

    [CustomPropertyDrawer(typeof(Debugging))]
    public class DebuggingDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            //EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.LabelField(position, "Debug ", EditorStyles.boldLabel);
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var textRect1 = new Rect(position.x + 80, position.y, 30, position.height);
            var amountRect = new Rect(position.x + 110, position.y, 30, position.height);
            var textRect2 = new Rect(position.x + 140, position.y, 60, position.height);
            var unitRect = new Rect(position.x + 200, position.y, 30, position.height);
            var textRect3 = new Rect(position.x + 230, position.y, 60, position.height);
            var nameRect = new Rect(position.x + 280, position.y, 30, position.height);
            

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(textRect1, "Log");
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("log"), GUIContent.none);
            EditorGUI.LabelField(textRect2, "Inspector");
            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("inspector"), GUIContent.none);
            EditorGUI.LabelField(textRect3, "Nothing");
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("nothing"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(AudioTrack))]
    public class AudioTrackDrawer : PropertyDrawer {



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);


                // Draw label
                //EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var textRect1 = new Rect(position.x + 100, position.y, 80, position.height);
            var amountRect = new Rect(position.x + 180, position.y, 80, position.height);
            var textRect2 = new Rect(position.x + 270, position.y, 100, position.height);
            var unitRect = new Rect(position.x + 370, position.y, 80, position.height);
            var textRect3 = new Rect(position.x + 500, position.y, 60, position.height);
            var nameRect = new Rect(position.x + 600, position.y, 30, position.height);

            EditorGUI.LabelField(position, $"{property.FindPropertyRelative("trackName").stringValue} ", EditorStyles.boldLabel);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(textRect1, "Track Name");
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("trackName"), GUIContent.none);
            EditorGUI.LabelField(textRect2, "Is Soundtracks?");
            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("soundtracks"), GUIContent.none);

            
            EditorGUILayout.PropertyField(property.FindPropertyRelative("audio"), new GUIContent($"{property.FindPropertyRelative("trackName").stringValue} Audio"));
            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(AudioObject))]
    public class AudioObjectDrawer : PropertyDrawer {



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            

            // Draw label
            //EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects


            float bufferAmount = (position.width - 40) / 8;

            float textBuffer = 0;


            var amountRect = new Rect(position.x  + 0, position.y, bufferAmount*3f, position.height);
            var unitRect = new Rect(position.x + 5  + bufferAmount * 3f, position.y, bufferAmount * 3f, position.height);
            var textRect3 = new Rect(position.x  + 10  + bufferAmount * 6f, position.y, 12, position.height);
            var nameRect2 = new Rect(position.x  + 22  + bufferAmount * 6f, position.y, bufferAmount * 1f, position.height);
            var textRect4 = new Rect(position.x  + 27 + bufferAmount * 7f, position.y, 12, position.height);
            var nameRect3 = new Rect(position.x + 39  + bufferAmount * 7f, position.y, bufferAmount * 1f, position.height);

            //EditorGUI.LabelField(new Rect(position.x, position.y, textBuffer, position.height), $"{property.FindPropertyRelative("nickname").stringValue} ", EditorStyles.boldLabel);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels

            
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("nickname"), GUIContent.none);
            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("name"), GUIContent.none);

            EditorGUI.LabelField(textRect3, "C");
            EditorGUI.PropertyField(nameRect2, property.FindPropertyRelative("clip"), GUIContent.none);
            EditorGUI.LabelField(textRect4, "I");
            EditorGUI.PropertyField(nameRect3, property.FindPropertyRelative("intro"), GUIContent.none);


            // Set indent back to what it was
            EditorGUI.indentLevel = indent;



            EditorGUI.EndProperty();
        }
    }

    [CustomEditor(typeof(AudioManager))]
    [CanEditMultipleObjects]
    public class AudioManagerEditor : Editor {

        SerializedProperty debugProp;
        SerializedProperty tracksProp;

        void OnEnable() {
            // Setup the SerializedProperties.
            debugProp = serializedObject.FindProperty("debug");
            tracksProp = serializedObject.FindProperty("tracks");
        }



        public override void OnInspectorGUI() {
            serializedObject.Update();

            AudioManager audioManager = (AudioManager)target;

            int songCount = 0;
            int sfxCount = 0;
            foreach (AudioTrack _track in audioManager.tracks) {
                if(_track.soundtracks) songCount += _track.audio.Length;
                else sfxCount += _track.audio.Length;
            }

            string songText = songCount == 1 ? "Song" : "Songs";
            string trackText = audioManager.tracks.Length == 1 ? "Track" : "Tracks";

            if (audioManager.debug.inspector == true) {
                GUILayout.Label($"{audioManager.tracks.Length} {trackText} | {songCount} {songText} | {sfxCount} SFX | Debug Mode");
            } else {
                GUILayout.Label($"{audioManager.tracks.Length} {trackText} | {songCount} {songText} | {sfxCount} SFX");
            }


            EditorGUILayout.PropertyField(debugProp);
            EditorGUILayout.PropertyField(tracksProp);
            serializedObject.ApplyModifiedProperties();
        }
    }

}


public enum AudioName {
    ST_001,
    ST_002,
    ST_003,
    ST_004,
    ST_005,
    SFU_01,
    SFU_02,
    SFU_03,
    None,
}



