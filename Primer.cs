using System.Collections.Generic;
using Bourne.BatchLoader.Model;

namespace Bourne.BatchLoader
{
    internal static class Primer
    {
        public static IEnumerable<DataSourceSlice> Prime(IEnumerable<DataSource> sources)
        {
            foreach (var dataSource in sources)
            {
                var sliceIndex = 0;
                foreach (var slice in GetDataSlices(dataSource))
                    yield return new DataSourceSlice($"{dataSource.LifetimeKey}_{++sliceIndex:D3}", slice, dataSource.LifetimeKey);
            }
        }

        public static IEnumerable<DataSource> GetSources()
        {
            yield return new DataSource("SCV_BUTLINS.BOOKING_GUEST");
        }

        private static IEnumerable<string> GetDataSlices(DataSource dataSource)
        {
            const int size = 10;
            for (var i = 0; i < size; i++)
                yield return $"select * from {dataSource.FullName} where mod(booking_id,{size}) = {i}";
        }

        /*
           yield return new DataSource("FCT_ESP_RESPONSE_TRANSMISSION", "FCT_ESP_RESPONSE_TRANSMISSION", "SELECT * FROM scv_butlins.FCT_ESP_RESPONSE_TRANSMISSION");
           yield return new DataSource("FCT_ESP_RESPONSE_OPEN", "FCT_ESP_RESPONSE_OPEN", "SELECT * FROM scv_butlins.FCT_ESP_RESPONSE_OPEN");
           yield return new DataSource("FCT_BOOKING_HIST", "FCT_BOOKING_HIST", "SELECT * FROM scv_butlins.FCT_BOOKING_HIST");
           yield return new DataSource("FCT_BOOKING_ADDON_HIST", "FCT_BOOKING_ADDON_HIST", "SELECT * FROM scv_butlins.FCT_BOOKING_ADDON_HIST");
           yield return new DataSource("FCT_INVOICE_ITEM_HIST", "FCT_INVOICE_ITEM_HIST", "SELECT * FROM scv_butlins.FCT_INVOICE_ITEM_HIST");
           yield return new DataSource("MOSAIC_POSTCODE", "MOSAIC_POSTCODE", "SELECT * FROM scv_butlins.MOSAIC_POSTCODE");
           yield return new DataSource("PREMIER_CLUB", "PREMIER_CLUB", "SELECT * FROM scv_butlins.PREMIER_CLUB");
           yield return new DataSource("MOSAIC_POSTCODE_DPS", "MOSAIC_POSTCODE_DPS", "SELECT * FROM scv_butlins.MOSAIC_POSTCODE_DPS");
        */
    }
}
