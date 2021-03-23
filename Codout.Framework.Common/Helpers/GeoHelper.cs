using System;

namespace Codout.Framework.Common.Helpers
{
    public class GeoHelper
    {
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
            /*
            The Haversine formula according to Dr. Math.
            http://mathforum.org/library/drmath/view/51879.html
                
            dlon = lon2 - lon1
            dlat = lat2 - lat1
            a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
            c = 2 * atan2(sqrt(a), sqrt(1-a)) 
            d = R * c
                
            Where
                * dlon is the change in longitude
                * dlat is the change in latitude
                * c is the great circle distance in Radians.
                * R is the radius of a spherical Earth.
                * The locations of the two points in 
                    spherical coordinates (longitude and 
                    latitude) are lon1,lat1 and lon2, lat2.
            */
            double dLat1InRad = latOri * (Math.PI / 180.0);
            double dLong1InRad = lngOri * (Math.PI / 180.0);
            double dLat2InRad = latDest * (Math.PI / 180.0);
            double dLong2InRad = lngDest * (Math.PI / 180.0);

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Asin(Math.Sqrt(a));

            // Distance.
            // const Double kEarthRadiusMiles = 3956.0;
            const Double kEarthRadiusKms = 6376.5;
            double dDistance = kEarthRadiusKms * c;

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

}
