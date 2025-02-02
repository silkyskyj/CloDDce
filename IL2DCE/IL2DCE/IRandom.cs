namespace IL2DCE
{
    public interface IRandom
    {
        int Next(int maxValue); 
        int Next(int minValue, int maxValue);
    }
}