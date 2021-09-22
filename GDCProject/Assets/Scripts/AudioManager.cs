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

        public int volume = 5;
        public bool mute;
        public AudioSource source;
        public AudioSource secondSource;
        public bool secondSourceActive;

        public AudioObject[] audio;

        public IEnumerator currentVolumeCoroutineSource;
        public IEnumerator currentVolumeCoroutineSecondSource;
    }

    private void Awake() {
        // awake always gets called before start. When you want one version of something put it in an awake since you can use then use it in start and you know its been created
        if (!instance) {
            instance = this;
        } else {
            Destroy(this);
        }
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
            GUI.contentColor = Color.HSVToRGB(40f / 360f, 0.4f, 0.8f);
            EditorGUI.LabelField(position, "Debug ", EditorStyles.boldLabel);
            GUI.contentColor = Color.white;
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

            GUI.contentColor = Color.white;


            // Draw label
            //EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float bufferAmount = (position.width - 120) / 8;

            // Calculate rects
            var textRect1 = new Rect(position.x + bufferAmount * 2, position.y, bufferAmount*2, position.height);
            var amountRect = new Rect(position.x + bufferAmount * 4, position.y, bufferAmount * 2, position.height);
            var textRect2 = new Rect(position.x + bufferAmount * 6, position.y, bufferAmount * 2, position.height);
            var unitRect = new Rect(position.x + bufferAmount * 8, position.y, 30, position.height);
            var textRect3 = new Rect(position.x + 500, position.y, 60, position.height);
            var nameRect = new Rect(position.x + 600, position.y, 30, position.height);

            GUI.contentColor = Color.HSVToRGB(60f / 360f, 0.7f, 1);
            EditorGUI.LabelField(new Rect(position.x, position.y, bufferAmount*2, position.height), $"{property.FindPropertyRelative("trackName").stringValue} ", EditorStyles.boldLabel);
            GUI.contentColor = Color.white;
            // Draw fields - passs GUIContent.none to each so they are drawn without labels

            EditorGUI.LabelField(textRect1, "Track Name");
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("trackName"), GUIContent.none);
            EditorGUI.LabelField(new Rect(textRect1.x, textRect1.y, textRect1.width + amountRect.width, textRect1.height + amountRect.height), new GUIContent("", "The name used to identify this track and other objects relating to it"));
            EditorGUI.LabelField(textRect2, "Is Soundtracks?");
            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("soundtracks"), GUIContent.none);
            EditorGUI.LabelField(new Rect(textRect2.x, textRect2.y, textRect2.width + unitRect.width, textRect2.height + unitRect.height), new GUIContent("", "Is this track used for music? (Only 1 soundtrack track allowed)"));

            var color = new Color();
            color = Color.HSVToRGB(80f / 360f, 0.5f, 0.8f);

            GUI.contentColor = Color.HSVToRGB(80f / 360f, 0.5f, 0.8f);
            //GUI.backgroundColor = color;
            color.a = 0.1f;
            //GUI.backgroundColor = color;
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
            GUI.contentColor = Color.white;


            // Draw label
            //EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects


            float bufferAmount = (position.width - 40) / 8;



            var amountRect = new Rect(position.x  + 0, position.y, bufferAmount*3f, position.height);
            var unitRect = new Rect(position.x + 5  + bufferAmount * 3f, position.y, bufferAmount * 3f, position.height);
            var textRect3 = new Rect(position.x  + 10  + bufferAmount * 6f, position.y, 12, position.height);
            var nameRect2 = new Rect(position.x  + 22  + bufferAmount * 6f, position.y, bufferAmount * 1f, position.height);
            var textRect4 = new Rect(position.x  + 27 + bufferAmount * 7f, position.y, 12, position.height);
            var nameRect3 = new Rect(position.x + 39  + bufferAmount * 7f, position.y, bufferAmount * 1f, position.height);

            //EditorGUI.LabelField(new Rect(position.x, position.y, textBuffer, position.height), $"{property.FindPropertyRelative("nickname").stringValue} ", EditorStyles.boldLabel);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels

            GUI.contentColor = Color.HSVToRGB(70f / 360f, 0.6f, 0.9f);
            GUI.backgroundColor = Color.HSVToRGB(70f / 360f, 0f, 0.8f);
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("nickname"), GUIContent.none);
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
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

                EditorGUILayout.PropertyField(debugProp);
                GUILayout.Label($"-----");
                GUIStyle style = new GUIStyle();
                style.richText = true;

                string masterMutedText = audioManager.masterMute ? "(Muted)" : "";
                GUILayout.Label($"Master Volume: {audioManager.masterVolume} {masterMutedText}");
                
                
                foreach (AudioTrack _track in audioManager.tracks) {
                    string soundtrackText = _track.soundtracks ? "(ST)" : "";
                    string mutedText = _track.mute ? "(Muted)" : "";
                    GUILayout.Label($"<size=15><color=#99992eFF><b>{_track.trackName} {soundtrackText}</b></color> </size> <color=#bebebeFF> VOL: {_track.volume} {mutedText} SOUNDS: {_track.audio.Length} </color>", style);
                    if(_track.source != null) {
                        string activeText = _track.secondSourceActive && _track.source.isPlaying ? "" : "(Active)";
                        if(_track.source.clip != null) {
                            GUILayout.Label($"---> Source TIME: {_track.source.time} VOL: {_track.source.volume} CLIP: {_track.source.clip.name} {activeText}");
                        } else {
                            GUILayout.Label($"---> Source TIME: {_track.source.time} VOL: {_track.source.volume} CLIP: NONE {activeText}");
                        }
                        
                    }
                    if (_track.secondSource != null) {
                        string activeText = _track.secondSourceActive && _track.source.isPlaying ? "(Active)" : "";
                        
                        if (_track.secondSource.clip != null) {
                            GUILayout.Label($"---> Source 2 TIME: {_track.secondSource.time} VOL: {_track.secondSource.volume} CLIP: {_track.secondSource.clip.name} {activeText}");
                        } else {
                            GUILayout.Label($"---> Source 2 TIME: {_track.secondSource.time} VOL: {_track.secondSource.volume} CLIP: NONE {activeText}");
                        }
                    }

                }
            } else {
                GUILayout.Label($"{audioManager.tracks.Length} {trackText} | {songCount} {songText} | {sfxCount} SFX");

                EditorGUILayout.PropertyField(debugProp);
                GUI.contentColor = Color.HSVToRGB(60f / 360f, 0.5f, 0.8f);
                EditorGUILayout.PropertyField(tracksProp);
            }


            

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
    FXU_01,
    FXU_02,
    FXU_03,
    None,
}



