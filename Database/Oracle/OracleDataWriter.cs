using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Bourne.BatchLoader.Database.Oracle
{
    internal class OracleDataWriter : IReaderWriter
    {
        private delegate void DoOutput(TextWriter writer, OracleDataReader rdr, int x);

        public static IReaderWriter Create(OracleDataReader reader)
        {
            return new OracleDataWriter(reader, GetTypes(reader));
        }

        private readonly OracleDataReader _reader;
        private readonly DoOutput[] _types;
        private int _rowsWritten;

        private OracleDataWriter(OracleDataReader reader, IEnumerable<DoOutput> outputs)
        {
            _reader = reader;
            _types = outputs.ToArray();
        }

        Task<bool> IReaderWriter.ReadAsync() => _reader.ReadAsync();

        int IReaderWriter.RowsWritten => _rowsWritten;

        void IReaderWriter.Write(TextWriter writer)
        {
            _types[0](writer, _reader, 0);
            for (var i = 1; i < _types.Length; i++)
            {
                writer.Write(',');
                _types[i](writer, _reader, i);
            }

            _rowsWritten++;
            writer.WriteLine();
        }

        void IReaderWriter.WriteHeaders(TextWriter writer)
        {
            for (var i = 0; i < _types.Length; i++)
            {
                if (i > 0) writer.Write(',');
                writer.Write('"');
                writer.Write(_reader.GetName(i));
                writer.Write('"');
            }

            writer.WriteLine();
        }

        private static IEnumerable<DoOutput> GetTypes(OracleDataReader reader)
        {
            for (var i = 0; i < reader.FieldCount; i++)
                yield return GetType(reader.GetDataTypeName(i));
        }

        private static DoOutput GetType(string type)
        {
            return type switch
            {
                "Int16" => OutputInt16,
                "Int32" => OutputInt32,
                "Int64" => OutputInt64,
                "Date" => OutputDateTime,
                "TimeStamp" => OutputTimeStamp,
                "TimeStampTZ" => OutputTimeStampTz,
                "Decimal" => OutputDecimal,
                "Double" => OutputDouble,
                "String" => OutputString,
                _ => OutputString
            };
        }

        private static void OutputDateTime(TextWriter writer, OracleDataReader rdr, int i)
        {
            var ts1 = rdr.GetOracleDate(i);
            if (ts1.IsNull) return;
            writer.Write(
                $"\"{ts1.Year:D4}-{ts1.Month:D2}-{ts1.Day:D2} {ts1.Hour:D2}:{ts1.Minute:D2}:{ts1.Second:D2}\"");
        }

        private static void OutputInt16(TextWriter writer, OracleDataReader rdr, int i)
        {
            if (rdr.IsDBNull(i)) return;
            writer.Write(rdr.GetInt16(i));
        }

        private static void OutputInt32(TextWriter writer, OracleDataReader rdr, int i)
        {
            if (rdr.IsDBNull(i)) return;
            writer.Write(rdr.GetInt32(i));
        }

        private static void OutputInt64(TextWriter writer, OracleDataReader rdr, int i)
        {
            if (rdr.IsDBNull(i)) return;
            writer.Write(rdr.GetInt64(i));
        }

        private static void OutputTimeStamp(TextWriter writer, OracleDataReader rdr, int i)
        {
            var ts1 = rdr.GetOracleTimeStamp(i);
            if (ts1.IsNull) return;
            writer.Write(
                $"\"{ts1.Year:D4}-{ts1.Month:D2}-{ts1.Day:D2} {ts1.Hour:D2}:{ts1.Minute:D2}:{ts1.Second:D2}.{ts1.Nanosecond}\"");
        }

        private static void OutputTimeStampTz(TextWriter writer, OracleDataReader rdr, int i)
        {
            var ts1 = rdr.GetOracleTimeStampTZ(i);
            if (ts1.IsNull) return;
            writer.WriteLine(
                $"\"{ts1.Year:D4}-{ts1.Month:D2}-{ts1.Day:D2} {ts1.Hour:D2}:{ts1.Minute:D2}:{ts1.Second:D2}.{ts1.Nanosecond} {ts1.TimeZone}\"");
        }

        private static void OutputDecimal(TextWriter writer, OracleDataReader rdr, int i)
        {
            var v = rdr.GetOracleDecimal(i);
            if (v.IsNull) return;
            writer.Write(v.Value);
        }

        private static void OutputDouble(TextWriter writer, OracleDataReader rdr, int i)
        {
            if (rdr.IsDBNull(i)) return;
            writer.Write(rdr.GetDouble(i));
        }

        private static void OutputString(TextWriter writer, OracleDataReader rdr, int i)
        {
            if (rdr.IsDBNull(i)) return;
            writer.Write('"');
            writer.Write(EncodeString(rdr.GetString(i)));
            writer.Write('"');
        }

        private static string EncodeString(string value)
            => value.IndexOf('"') == -1 ? value : value.Replace("\"", "\"\"");

        public void Dispose()
            => _reader.Dispose();
    }
}
