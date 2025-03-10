namespace IL2DCE
{
    class Random : System.Random, IRandom
    {
        public static Random Default = new Random();
    }
}
