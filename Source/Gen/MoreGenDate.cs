using RimWorld;

namespace ProgressRenderer
{

    public static class MoreGenDate
    {

        public static int QuadrumInteger(long absTicks, float longitude)
        {
            Quadrum quadrum = GenDate.Quadrum(absTicks, longitude);
            if (quadrum == Quadrum.Aprimay) { return 1; }
            else if (quadrum == Quadrum.Jugust) { return 2; }
            else if (quadrum == Quadrum.Septober) { return 3; }
            else if (quadrum == Quadrum.Decembary) { return 4; }
            else { return 0; }
        }
        
        public static int HoursPassedInteger(int absTicks, float longitude)
        {
            int ticks = absTicks + LocalTicksOffsetFromLongitude(longitude);
            return ticks / 2500;
        }

        private static int LocalTicksOffsetFromLongitude(float longitude)
        {
            return RimWorld.GenDate.TimeZoneAt(longitude) * 2500;
        }

    }

}
