using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilCache.Test
{
    public class KPIReport
    {
        public string PrimaryID { get; set; }
        public string GroupID { get; set; }
        public double KpiScore { get; set; }
        public double KpiTx
        {
            get
            {
                var tx = this.Items.Sum(x => x.Tx);
                return tx;
            }
        }

        [JsonConverter(typeof(DecimalConverter))]
        public decimal KpiRev
        {
            get
            {
                try
                {
                    var rev = this.Items.Sum(x => x.Rev);
                    return rev;
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"KipRev failed to convert: {ex}");
                    return 0;
                }
            }
        }

        public double KpiScoreNormalized
        {
            get
            {
                var score = this.Items.Where(x => x.Tx > 20d).Sum(x => x.Normalized);
                return score;
            }
        }

        public string Display { get; set; }
        public string FullName { get; set; }

        public List<KPIReportItem> Items { get; set; }

        public string District { get; set; }
        public string Division { get; set; }
        public string Region { get; set; }
        public string Territory { get; set; }
    }

    public class KPIReportItem
    {
        public DateTime Date { get; set; }
        public double Score { get; set; }
        public double Tx { get; set; }
        [JsonConverter(typeof(DecimalConverter))]
        public decimal Rev { get; set; }
        public double Normalized
        {
            get
            {
                if (this.Tx < 20d)
                    return 0;
                return Math.Round(this.Score / this.Tx * 100d);
            }
        }
        public int Order { get; set; }
    }
}
