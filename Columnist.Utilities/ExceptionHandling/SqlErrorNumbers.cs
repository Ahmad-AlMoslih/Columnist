namespace Columnist.Utilities.ExceptionHandling;

public static class SqlErrorNumbers
{
    public const int DuplicateKey = 2627;
    public const int DuplicateIndex = 2601;
    public const int ForeignKeyConflict = 547;
    public const int TextTooLong = 8152;
    public const int TextTooLongColumn = 2628;
    public const int Timeout = -2;
    public const int ServerNotFound = 53;
    public const int ConnectionFailed = -1;
}
