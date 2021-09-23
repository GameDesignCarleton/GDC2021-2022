using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class AudioManager : MonoBehaviour {

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
        public AudioName currentAudio;
        public AudioName currentSecondAudio;
    }

    [System.Serializable]
    private class AudioJob {
        public AudioAction action;
        public AudioName name;
        public bool fade;
        public float delay;

        public AudioJob(AudioAction _action, AudioName _name, bool _fade, float _delay) {
            action = _action;
            name = _name;
            fade = _fade;
            delay = _delay;
        }
    }

    public void setMasterMute(bool muteState) {
        if (masterMute != muteState) {
            masterMute = muteState;
            foreach (AudioTrack _track in tracks) {
                //if (muteState != _track.mute) {
                if (_track.source && !_track.secondSourceActive) {
                    if (muteState) {
                        if (_track.currentVolumeCoroutineSource != null) StopCoroutine(_track.currentVolumeCoroutineSource);
                        _track.currentVolumeCoroutineSource = FadeAudio(_track.source, _track.source.volume, false, 1f, _track);
                        StartCoroutine(_track.currentVolumeCoroutineSource);
                    } else {
                        if (_track.currentVolumeCoroutineSource != null) StopCoroutine(_track.currentVolumeCoroutineSource);
                        _track.currentVolumeCoroutineSource = FadeAudio(_track.source, _track.source.volume, true, 1f, _track);
                        StartCoroutine(_track.currentVolumeCoroutineSource);
                    }
                }

                if (_track.secondSource && _track.secondSourceActive) {
                    if (muteState) {
                        if (_track.currentVolumeCoroutineSecondSource != null) StopCoroutine(_track.currentVolumeCoroutineSecondSource);
                        _track.currentVolumeCoroutineSecondSource = FadeAudio(_track.secondSource, _track.secondSource.volume, false, 1f, _track);
                        StartCoroutine(_track.currentVolumeCoroutineSecondSource);

                    } else {
                        if (_track.currentVolumeCoroutineSecondSource != null) StopCoroutine(_track.currentVolumeCoroutineSecondSource);
                        _track.currentVolumeCoroutineSecondSource = FadeAudio(_track.secondSource, _track.secondSource.volume, true, 1f, _track);
                        StartCoroutine(_track.currentVolumeCoroutineSecondSource);
                    }
                }
                // }
            }
        } else {
            masterMute = muteState;
        }

    }


    private IEnumerator FadeAudio(AudioSource source, float _initial, bool fadeState, float _duration, AudioTrack _track) {
        float _volumeMult = ((float)_track.volume / 10) * ((float)masterVolume / 10);
        _duration /= 1.5f;
        if ((_track.mute || masterMute)) {
            _volumeMult = 0;
        }
        float _target;
        //_initial *= _volumeMult;
        float _timer = 0f;
        //float _target = 1f* _volumeMult - _initial;
        if (fadeState) {
            _target = _volumeMult;
        } else {
            _target = 0f;
        }

        while (_timer <= _duration) {
            source.volume = Mathf.Lerp(_initial, _target, _timer / _duration);
            _timer += Time.deltaTime;
            yield return null;
        }

    }

    private enum AudioAction {
        START,
        STOP,
    }

    private IEnumerator RunAudioJob(AudioJob _job) {
        yield return new WaitForSeconds(_job.delay);


        AudioTrack _track = (AudioTrack)audioTable[_job.name];


        switch (_job.action) {
            case AudioAction.START:
                float _volumeMult = ((float)_track.volume / 10) * ((float)masterVolume / 10);

                if ((_track.mute || masterMute)) {
                    _volumeMult = 0;
                }

                if (_track.secondSourceActive) {
                    _track.source.clip = GetAudioFromAudioTrack(_job.name, _track).clip;
                    _track.source.Play();
                    _track.currentAudio = GetAudioFromAudioTrack(_job.name, _track).name;
                    _track.secondSourceActive = false;

                    //_track.source.mute = _track.mute == true || masterMute == true;




                    if (!_job.fade) {
                        _track.source.volume = _volumeMult;
                        _track.secondSource.Stop();
                        _track.secondSource.clip = null;
                    } else {
                        if (_track.currentVolumeCoroutineSource != null) StopCoroutine(_track.currentVolumeCoroutineSource);
                        _track.currentVolumeCoroutineSource = FadeAudio(_track.source, 0f, true, 1f, _track);
                        StartCoroutine(_track.currentVolumeCoroutineSource);

                        if (_track.currentVolumeCoroutineSecondSource != null) StopCoroutine(_track.currentVolumeCoroutineSecondSource);
                        _track.currentVolumeCoroutineSecondSource = FadeAudio(_track.secondSource, _track.secondSource.volume, false, 1f, _track);
                        StartCoroutine(_track.currentVolumeCoroutineSecondSource);

                    }
                } else {
                    _track.secondSource.clip = GetAudioFromAudioTrack(_job.name, _track).clip;
                    _track.secondSource.Play();
                    _track.currentSecondAudio = GetAudioFromAudioTrack(_job.name, _track).name;
                    _track.secondSourceActive = true;

                    //_track.secondSource.mute = _track.mute == true || masterMute == true;

                    if (!_job.fade) {
                        _track.secondSource.volume = _volumeMult;
                        _track.source.Stop();
                        _track.source.clip = null;
                    } else {
                        if (_track.currentVolumeCoroutineSource != null) StopCoroutine(_track.currentVolumeCoroutineSource);
                        _track.currentVolumeCoroutineSource = FadeAudio(_track.source, _track.source.volume, false, 1f, _track);
                        StartCoroutine(_track.currentVolumeCoroutineSource);

                        if (_track.currentVolumeCoroutineSecondSource != null) StopCoroutine(_track.currentVolumeCoroutineSecondSource);
                        _track.currentVolumeCoroutineSecondSource = FadeAudio(_track.secondSource, 0f, true, 1f, _track);
                        StartCoroutine(_track.currentVolumeCoroutineSecondSource);

                    }
                }

                break;

            case AudioAction.STOP:
                if (!_job.fade) {
                    _track.source.Stop();
                }

                break;
        }

        //CHANGE
        //_track.source.time = 30f;
        //Log(_track.source.clip.length.ToString());
        //audioSource.time;


        jobTable.Remove(_job.name);
        Log("Job count: " + jobTable.Count);
        yield return null;
    }

    private void AddJob(AudioJob _job) {
        //remove conflicting jobs
        RemoveConflictingJobs(_job.name);

        //start jobPl
        IEnumerator _jobRunner = RunAudioJob(_job);
        jobTable.Add(_job.name, _jobRunner);
        StartCoroutine(_jobRunner);
        Log("Starting job on [" + _job.name + "] with operation" + _job.action + $"(Job count: {jobTable.Count})");
    }

    private void RemoveJob(AudioName _name) {
        if (!jobTable.ContainsKey(_name)) {
            LogWarning("Trying to stop a job [" + _name + "] that is not running.");
            return;
        }

        IEnumerator _runningJob = (IEnumerator)jobTable[_name];
        StopCoroutine(_runningJob);
        jobTable.Remove(_name);
    }

    private void RemoveConflictingJobs(AudioName _name) {
        /*if !(jobTable.ContainsKey(_name)) {
            RemoveConflictingJobs(_name);
        }*/

        AudioName _conflictAudio = AudioName.None;
        foreach (DictionaryEntry _entry in jobTable) {
            AudioName _audioName = (AudioName)_entry.Key;
            AudioTrack _audioTrackInUse = (AudioTrack)audioTable[_audioName];
            AudioTrack _audioTrackNeeded = (AudioTrack)audioTable[_name];
            if (_audioTrackNeeded.source == _audioTrackInUse.source) {
                //conflict
                _conflictAudio = _audioName;
            }
        }
        if (_conflictAudio != AudioName.None) {
            RemoveJob(_conflictAudio);
        }
    }


    #region Public Functions

    public void PlayAudio(AudioName _type, bool _fade = false, float _delay = 0.0f) {
        AddJob(new AudioJob(AudioAction.START, _type, _fade, _delay));
    }

    public void StopAudio(AudioName _type, bool _fade = false, float _delay = 0.0f) {
        AddJob(new AudioJob(AudioAction.STOP, _type, _fade, _delay));
    }

    public AudioObject GetAudioFromAudioTrack(AudioName _name, AudioTrack _track) {
        foreach (AudioObject _obj in _track.audio) {
            if (_obj.name == _name) {
                return _obj;
            }
        }
        return null;
    }

    

    #endregion

    #region Private Functions

    private void Configure() {
        instance = this;
        audioTable = new Hashtable();
        jobTable = new Hashtable();
        GenerateAudioTable();
    }

    private void Dispose() {
        foreach (DictionaryEntry _entry in jobTable) {
            IEnumerator _job = (IEnumerator)_entry.Value;
            StopCoroutine(_job);
        }
    }

    private void GenerateAudioTable() {
        foreach (AudioTrack _track in tracks) {
            _track.volume = 5; //TEMP
            foreach (AudioObject _obj in _track.audio) {
                //do not duAplicate keys, check if something already exists before adding it into here
                if (audioTable.ContainsKey(_obj.name)) {
                    LogWarning($"You are trying to register audio [{_obj.name}] on {_track.trackName} that has already been registered somewhere");
                } else {
                    audioTable.Add(_obj.name, _track);
                    Log($"Registering audio [{_obj.name}] on {_track.trackName}");
                }
            }
        }
    }

    #endregion

    #region Start Stop

    private void Awake() {
        // awake always gets called before start. When you want one version of something put it in an awake since you can use then use it in start and you know its been created
        if (!instance) {
            Configure();
        } else {
            Destroy(this);
        }
    }

    private void OnDisable() {
        //dispose of coroutines in the event the object is deleted(to avoid things running in the background and potentionally causing a memory leak)
        Dispose();
    }


    #endregion

    #region Log Functions
    private void Log(string _message, bool shouldSendLog = false) {
        shouldSendLog = shouldSendLog || debug.log;
        if (shouldSendLog) Debug.Log($"[Audio Manager]: {_message}");
    }

    private void LogWarning(string _message) {
        Debug.LogWarning($"[Audio Manager]: {_message}");
    }

    private void LogError(string _message) {
        Debug.LogError($"[Audio Manager]: {_message}");
    }
    #endregion


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
            var textRect1 = new Rect(position.x + bufferAmount * 2, position.y, bufferAmount * 2, position.height);
            var amountRect = new Rect(position.x + bufferAmount * 4, position.y, bufferAmount * 2, position.height);
            var textRect2 = new Rect(position.x + bufferAmount * 6, position.y, bufferAmount * 2, position.height);
            var unitRect = new Rect(position.x + bufferAmount * 8, position.y, 30, position.height);
            var textRect3 = new Rect(position.x + 500, position.y, 60, position.height);
            var nameRect = new Rect(position.x + 600, position.y, 30, position.height);

            GUI.contentColor = Color.HSVToRGB(60f / 360f, 0.7f, 1);
            EditorGUI.LabelField(new Rect(position.x, position.y, bufferAmount * 2, position.height), $"{property.FindPropertyRelative("trackName").stringValue} ", EditorStyles.boldLabel);
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



            var amountRect = new Rect(position.x + 0, position.y, bufferAmount * 3f, position.height);
            var unitRect = new Rect(position.x + 5 + bufferAmount * 3f, position.y, bufferAmount * 3f, position.height);
            var textRect3 = new Rect(position.x + 10 + bufferAmount * 6f, position.y, 12, position.height);
            var nameRect2 = new Rect(position.x + 22 + bufferAmount * 6f, position.y, bufferAmount * 1f, position.height);
            var textRect4 = new Rect(position.x + 27 + bufferAmount * 7f, position.y, 12, position.height);
            var nameRect3 = new Rect(position.x + 39 + bufferAmount * 7f, position.y, bufferAmount * 1f, position.height);

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
                if (_track.soundtracks) songCount += _track.audio.Length;
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
                    if (_track.source != null) {
                        string activeText = _track.secondSourceActive && _track.source.isPlaying ? "" : "(Active)";
                        if (_track.source.clip != null) {
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



