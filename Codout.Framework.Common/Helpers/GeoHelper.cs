using System;

namespace Codout.Framework.Common.Helpers;

public class GeoHelper
{
    const double KEarthRadiusKms = 6376.5;

    /// <summary>
    /// Obtem a distância de dois pontos em Kilometros
    /// </summary>
    /// <param name="latOri">Latitude de origem</param>
    /// <param name="lngOri">Longitude de origem</param>
    /// /// <param name="latDest">Latitude de destino</param>
    /// <param name="lngDest">Longitude de destino</param>
    /// <returns>Retorna a distância em Kilometros</returns>
    public static double Distance(double latOri, double lngOri, double latDest, double lngDest)
    {
        var dLat1InRad = latOri * (Math.PI / 180.0);
        var dLong1InRad = lngOri * (Math.PI / 180.0);
        var dLat2InRad = latDest * (Math.PI / 180.0);
        var dLong2InRad = lngDest * (Math.PI / 180.0);

        var dLongitude = dLong2InRad - dLong1InRad;
        var dLatitude = dLat2InRad - dLat1InRad;

        var a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

        var c = 2.0 * Math.Asin(Math.Sqrt(a));

        var dDistance = KEarthRadiusKms * c;

        return dDistance;
    }

    public static string ConvertToDegrees(double lat, double lon)
    {
        var latDir = (lat >= 0 ? "N" : "S");
        lat = Math.Abs(lat);
        var latMinPart = ((lat - Math.Truncate(lat) / 1) * 60);
        var latSecPart = ((latMinPart - Math.Truncate(latMinPart) / 1) * 60);

        var lonDir = (lon >= 0 ? "E" : "W");
        lon = Math.Abs(lon);
        var lonMinPart = ((lon - Math.Truncate(lon) / 1) * 60);
        var lonSecPart = ((lonMinPart - Math.Truncate(lonMinPart) / 1) * 60);

        return $"{Math.Truncate(lat)}°{Math.Truncate(latMinPart)}'{Math.Truncate(latSecPart)}\"{latDir} {Math.Truncate(lon)}°{Math.Truncate(lonMinPart)}'{Math.Truncate(lonSecPart)}\"{lonDir}";
    }
}

