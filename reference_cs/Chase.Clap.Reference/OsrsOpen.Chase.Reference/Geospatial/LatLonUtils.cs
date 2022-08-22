namespace OsrsOpen.Chase.Reference.Geospatial
{
    public static class LatLonUtils
    {
        private const double meanRadius = 6371000.0; //m

        /// <summary>degrees per radian (180/pi)</summary>
        public const double DegreesPerRadian = (180.0d / Math.PI);
        /// <summary>radians per degree (pi/180)</summary>
        public const double RadiansPerDegree = 0.017453292519943295769236907684886127134428718885417d;

        /// <summary>
        /// Compute the Cartesian distance between two coordinates using the law of cosines method (fast, low accuracy)
        /// </summary>
        /// <param name="fromX">source X ordinate</param>
        /// <param name="fromY">source Y ordinate</param>
        /// <param name="toX">destination X ordinate</param>
        /// <param name="toY">destination Y ordinate</param>
        /// <returns>The great circle distance from a to b</returns>
        public static double Distance(double fromX, double fromY, double toX, double toY)
        {
            fromX = ToRadians(fromX);
            fromY = ToRadians(fromY);
            toX = ToRadians(toX);
            toY = ToRadians(toY);
            double a = Math.Acos(Math.Sin(fromY) * Math.Sin(toY) + Math.Cos(fromY) * Math.Cos(toY) * Math.Cos(toX - fromX));
            return a * meanRadius;
        }

        public static double ToDegrees(double radians)
        {
            return radians * DegreesPerRadian;
        }

        public static double ToRadians(double degrees)
        {
            return degrees * RadiansPerDegree;
        }
    }
}
