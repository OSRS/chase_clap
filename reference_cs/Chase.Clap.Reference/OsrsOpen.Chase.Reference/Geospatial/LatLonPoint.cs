using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsrsOpen.Chase.Reference.Geospatial
{
    public sealed class LatLonPoint
    {
        private readonly double lat;
        public double Lat
        {
            get { return this.lat; }
        }

        private readonly double lon;
        public double Lon
        {
            get { return this.lon; }
        }

        public LatLonPoint(double lon, double lat)
        {
            this.lat = lat;
            this.lon = lon;
        }

        public double Distance(LatLonPoint other)
        {
            if (other != null)
                return LatLonUtils.Distance(this.lon, this.lat, other.lon, other.lat);
            return double.NaN;
        }
    }
}
