using System;
using System.ComponentModel;
using System.Globalization;

namespace Codout.Framework.Common.Helpers;

public class LatLong
{
    private string _round4 = "0.0000";
    private string _round6 = "0.000000";
    private string _round8 = "0.00000000";

    public bool IsNegative;
    public double DecimalDegrees;
    public double DecimalMinutes;
    public double DecimalSeconds;
    public int Degrees;
    public int Minutes;
    public bool IsOk = true;
    public double EpsilonDeg = 0.0000000001;

    public enum CoordinateType
    {
        Longitude,
        Latitude,
        Undefined
    }

    public enum CoordinateFormat
    {
        [Description("Deg.dec")]
        D,
        [Description("Deg Min.dec")]
        DM,
        [Description("Deg Min Sec.dec")]
        DMS
    }

    public static LatLong FromDecimalDegree(double decdeg)
    {
        try
        {
            // checks, make sure they are in the range -180 to 180
            if (decdeg < -180 || decdeg > 180)
                throw new Exception("Latlong out of range -180 to 180");

            var ll = new LatLong();

            if (decdeg < 0)
                ll.IsNegative = true;
            decdeg = Math.Abs(decdeg);

            // get the degree
            ll.DecimalDegrees = decdeg;
            ll.Degrees = Convert.ToInt32(Math.Floor(ll.DecimalDegrees));
            var delta = ll.DecimalDegrees - ll.Degrees;

            // get the minutes
            ll.DecimalMinutes = delta * 60;
            ll.Minutes = Convert.ToInt32(Math.Floor(ll.DecimalMinutes));
            delta = ll.DecimalMinutes - ll.Minutes;

            // get the seconds
            ll.DecimalSeconds = delta * 60;

            return ll;
        }
        catch
        {
            return Empty();
        }
    }

    public static LatLong FromDegreeDecimalMinutes(int degree, double minute)
    {
        try
        {
            // checks, make sure they are in the range -180 to 180
            if (degree is < -180 or > 180)
                throw new Exception("Latlong out of range -180 to 180");

            var ll = new LatLong();

            if (degree < 0)
                ll.IsNegative = true;
            degree = Math.Abs(degree);
            minute = Math.Min(Math.Max(Math.Abs(minute), 0), 60 - ll.EpsilonDeg);

            ll.DecimalMinutes = minute;
            ll.Minutes = Convert.ToInt32(Math.Floor(minute));
            var delta = ll.DecimalMinutes - ll.Minutes;

            ll.DecimalSeconds = delta * 60;
            ll.Degrees = Convert.ToInt32(Math.Floor((double)degree));
            ll.DecimalDegrees = degree + (minute / 60);

            return ll;
        }
        catch
        {
            return Empty();
        }
    }

    public static LatLong FromDegreeMinutesDecimalSeconds(int degree, int minute, double second)
    {
        try
        {
            // checks, make sure they are in the range -180 to 180
            if (degree < -180 || degree > 180)
                throw new Exception("Latlong out of range -180 to 180");

            var ll = new LatLong();

            if (degree < 0)
                ll.IsNegative = true;
            degree = Math.Abs(degree);
            minute = Math.Min(Math.Max(Math.Abs(minute), 0), 59);
            second = Math.Min(Math.Max(Math.Abs(second), 0), 60 - ll.EpsilonDeg);

            ll.Degrees = Convert.ToInt32(Math.Floor((double)degree));
            ll.Minutes = Convert.ToInt32(Math.Floor((double)minute));
            ll.DecimalDegrees = degree + (minute / (double)60) + (second / 3600);
            ll.DecimalMinutes = minute + (second / 60);
            ll.DecimalSeconds = second;

            return ll;
        }
        catch
        {
            return Empty();
        }
    }

    public static LatLong Empty()
    {
        var em = new LatLong
        {
            IsOk = false
        };
        return em;
    }

    public override string ToString()
    {
        return IsNegative ? (-1 * DecimalDegrees).ToString(CultureInfo.InvariantCulture) : DecimalDegrees.ToString(CultureInfo.InvariantCulture);
    }

    public string ToStringFullyDecorated(CoordinateType latlongType)
    {
        var sign = latlongType switch
        {
            CoordinateType.Longitude => IsNegative ? "W" : "E",
            CoordinateType.Latitude => IsNegative ? "S" : "N",
            _ => IsNegative ? "-" : ""
        };

        CorrectMinuteOrSecondIs60(CoordinateFormat.DMS);

        string s;

        if (sign == "-" || sign == "")
            s = $"{sign}{Degrees:000}° {Minutes:00}' {DecimalSeconds.ToString(_round4).PadLeft(7, '0')}\"";
        else
            s = $"{Degrees:000}° {Minutes:00}' {DecimalSeconds.ToString(_round4).PadLeft(7, '0')}\" {sign}";

        return s;
    }

    public string ToStringFullyDecorated()
    {
        return ToStringFullyDecorated(LatLong.CoordinateType.Undefined);
    }

    public string ToStringDMS(bool decorate)
    {
        // decimal seconds will be rounded to 4 decimals, more is only apparent accuracy
        var sign = "";

        if (IsNegative)
            sign = "-";

        CorrectMinuteOrSecondIs60(CoordinateFormat.DMS);

        var s = decorate
            ? $"{sign}{Degrees,3}° {Minutes,2}' {DecimalSeconds.ToString(_round4),7}\"" 
            : $"{sign}{Degrees,3} {Minutes,2} {DecimalSeconds.ToString(_round4),7}";

        return s;
    }

    public string ToStringDMS()
    {
        return ToStringDMS(false);
    }

    public string ToStringDM(bool decorate)
    {
        // decimal minutes will be rounded to 6 decimals, more is only apparent accuracy
        var sign = "";

        if (IsNegative)
            sign = "-";

        CorrectMinuteOrSecondIs60(CoordinateFormat.DM);

        var s = decorate 
            ? $"{sign}{Degrees,3}° {DecimalMinutes.ToString(_round6),9}'" 
            : $"{sign}{Degrees,3} {DecimalMinutes.ToString(_round6),9}";
        return s;
    }

    public string ToStringDM()
    {
        return ToStringDM(false);
    }

    public string ToStringD(bool decorate)
    {
        // decimal degrees will be rounded to 8 decimals, more is only apparent accuracy
        var sign = "";
        
        if (IsNegative)
            sign = "-";

        var s = decorate 
            ? $"{sign}{DecimalDegrees.ToString(_round8),12}°" 
            : $"{sign}{DecimalDegrees.ToString(_round8),12}";
        
        return s;
    }

    public string ToStringD()
    {
        return ToStringD(false);
    }

    public bool CorrectMinuteOrSecondIs60(CoordinateFormat coordinateFormat)
    {
        switch (coordinateFormat)
        {
            // nothing to do
            case CoordinateFormat.D:
                return true;
            case CoordinateFormat.DM:
            {
                // check if min is 60
                if (MinuteOrSecondIs60(DecimalMinutes, _round6) == 1)
                {
                    Degrees += 1;
                    Minutes = 0;
                }
                return true;
            }
            case CoordinateFormat.DMS:
            {
                // check if sec is 60
                if (MinuteOrSecondIs60(DecimalSeconds, _round4) == 1)
                {
                    Minutes += 1;
                    DecimalSeconds = 0;
                    // cascades?
                    if (Minutes == 60)
                    {
                        Degrees += 1;
                        Minutes = 0;
                    }
                }
                return true;
            }
            default:
                return false;
        }
    }

    private int MinuteOrSecondIs60(double minuteOrSec, string roundStr)
    {
        try
        {
            var minestrone = minuteOrSec.ToString(roundStr);

            if (Convert.ToDouble(minestrone) >= 60 - EpsilonDeg && Convert.ToDouble(minestrone) <= 60 + EpsilonDeg)
                return 1;
            
            return 0;
        }
        catch (Exception)
        {
            return -1;
        }
    }

    private int MinuteOrSecondIs60(double minuteOrSec, int nrOfDec)
    {
        nrOfDec = nrOfDec switch
        {
            > 8 => 8,
            < 0 => 0,
            _ => nrOfDec
        };

        var rstr = "0";
        
        if (nrOfDec > 0)
            rstr = "0." + new string('0', nrOfDec);
        
        return MinuteOrSecondIs60(minuteOrSec, rstr);
    }
}
