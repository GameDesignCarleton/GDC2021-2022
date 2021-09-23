using System;

[Serializable]
public class DateTime
{
    private int _minute;
    private int _hour;
    private int _day;

    public DateTime(int day=0, int hour=0, int minute=0) {
        this.day = day;
        this.hour = hour;
        this.minute = minute;
    }
    public int minute {
        get {
            return _minute;
        }
        set {
            _minute = value;
            while (_minute >= 60) {
                _minute -= 60;
                hour++;
            }
        }
    }
    public int hour {
        get {
            return _hour;
        }
        set {
            _hour = value;
            while (_hour >= 24) {
                _hour -= 24;
                day++;
            }
        }
    }
    public int day {
        get {
            return _day;
        }
        set {
            _day = value;
        }
    }

    public static DateTime operator -(DateTime a, DateTime b) {
        return new DateTime(a.day-b.day, a.hour-b.hour, a.minute-b.minute);
    }

    public static bool operator >(DateTime a, DateTime b) {
        if (a.day != b.day) {
            return a.day > b.day;
        }
        if (a.hour != b.hour) {
            return a.hour > b.hour;
        }
        if (a.minute != b.minute) {
            return a.minute > b.minute;
        }
        return false; //the two are equal
    }
    public static bool operator <(DateTime a, DateTime b) {
        if (a.day != b.day) {
            return a.day < b.day;
        }
        if (a.hour != b.hour) {
            return a.hour < b.hour;
        }
        if (a.minute != b.minute) {
            return a.minute < b.minute;
        }
        return false; //the two are equal
    }
    public static bool operator ==(DateTime a, DateTime b) {
        return (a.day == b.day && a.hour == b.hour && a.minute == b.minute);
    }
    public static bool operator !=(DateTime a, DateTime b) {
        return !(a == b);
    }
    public static bool operator >=(DateTime a, DateTime b) {
        return (a > b || a == b);
    }
    public static bool operator <=(DateTime a, DateTime b) {
        return (a < b || a == b);
    }

    public override bool Equals(object obj)
    {
        //auto-generated
        return obj is DateTime time &&
               _minute == time._minute &&
               _hour == time._hour &&
               _day == time._day;
    }

    public override int GetHashCode()
    {
        //auto-generated
        int hashCode = 895923327;
        hashCode = hashCode * -1521134295 + _minute.GetHashCode();
        hashCode = hashCode * -1521134295 + _hour.GetHashCode();
        hashCode = hashCode * -1521134295 + _day.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return "DateTime: day=" + day + " hour=" + hour + " minute=" + minute;
    }

    public DateTime TimeOnly() {
        return new DateTime(0, hour, minute);
    }
}