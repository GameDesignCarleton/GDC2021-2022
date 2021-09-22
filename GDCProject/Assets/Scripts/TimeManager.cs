using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public double secondsPerInGameMinute = 1.0f; //placeholder, set in inspector
    private double timeSinceLastMinute;
    private DateTime dayStart = new DateTime(0, 6, 0); //inclusive
    private DateTime dayEnd = new DateTime(0, 22, 0); //exclusive
    public DateTime currentTime {
        get;
        set;
    }
    public bool timerPaused {
        get;
        set;
    }
    void Start()
    {
        timeSinceLastMinute = 0;
        dayStart = dayStart.TimeOnly();
        dayEnd = dayEnd.TimeOnly();
        InitializeTime();
    }
    void Update()
    {
        if (!timerPaused) {
            timeSinceLastMinute += Time.deltaTime;
            while (timeSinceLastMinute > secondsPerInGameMinute) {
                timeSinceLastMinute -= secondsPerInGameMinute;
                currentTime.minute++;
                // Debug.Log(currentTime.ToString() + " " + IsDay());
            }
        }
    }
    private void InitializeTime() {
        currentTime = new DateTime(0, 0, 0); //initialize time from json later
    }
    public static TimeManager GetTimeManager() {
        return GameObject.FindObjectOfType<TimeManager>();
    }
    public bool IsDay() {
        DateTime time = currentTime.TimeOnly();
        return (time >= dayStart && time < dayEnd);
    }
}