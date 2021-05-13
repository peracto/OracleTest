using System.IO;
using Oracle.ManagedDataAccess.Types;

namespace OracleTest.Database.Oracle
{
    internal static class AppExtensions
    {
        public static void TextWrite(this TextWriter writer, OracleDate ts1)
        {
            writer.Write($"\"{ts1.Year:D4}-{ts1.Month:D2}-{ts1.Day:D2} {ts1.Hour:D2}:{ts1.Minute:D2}:{ts1.Second:D2}\"");
        }

        public static void TextWrite(this TextWriter writer, OracleTimeStamp ts1)
        {
            writer.Write($"\"{ts1.Year:D4}-{ts1.Month:D2}-{ts1.Day:D2} {ts1.Hour:D2}:{ts1.Minute:D2}:{ts1.Second:D2}.{ts1.Nanosecond}\"");
        }
        public static void TextWrite(this TextWriter writer, OracleTimeStampTZ ts1)
        {
            writer.WriteLine($"\"{ts1.Year:D4}-{ts1.Month:D2}-{ts1.Day:D2} {ts1.Hour:D2}:{ts1.Minute:D2}:{ts1.Second:D2}.{ts1.Nanosecond} {ts1.TimeZone}\"");
        }
        public static void TextWrite(this TextWriter writer, OracleDecimal v)
        {
            writer.Write(v.Value);
        }
    }
}
