namespace SimpleBinaryFormat
{
    public enum DataType : byte
    {
        RegionBoundry,
        SubRegion,
        RegionArray,
        Simple,
        SimpleArray,
    }
    public enum TransitionType : byte
    {
        RegionStart,
        StreamEnd,
    }
}
