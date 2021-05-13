using System.Collections.Generic;
using System.Linq;

namespace OracleTest
{
    static class Primer
    {
        public static void Prime(PipelineQueue<DataSourceSlice> queue)
        {
            foreach (var query in GetDataSlices(GetSources()))
                queue.Enqueue(query);
        }

        static IEnumerable<DataSource> GetSources()
        {
            yield return new DataSource("SCV_BUTLINS", "BOOKING_GUEST");
        }

        static IEnumerable<DataSourceSlice> GetDataSlices(IEnumerable<DataSource> dataSources)
        {
            foreach (var dataSource in dataSources)
            {
                var queries = dataSource.CreateQueries(10).ToList();
                for (var i = 0; i < queries.Count; i++)
                    yield return new DataSourceSlice(dataSource, $"{dataSource.FullName}_{i + 1:D3}", queries[i]);
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
}
