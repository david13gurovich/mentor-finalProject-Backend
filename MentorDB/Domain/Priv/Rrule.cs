using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Priv
{
    public class Rrule
    {
        private const string DAILY = "Daily";
        private const string WEEKLY = "Weekly";
        private const string MONTHLY = "Monthly";
        private const string YEARLY = "Yearly";
        private const int DEFAUL_STARTING_VALUE = 0;
        private const int DEFAUL_STARTING_LENGTH = 2;
        private const int FIRST_INDEX = 0;
        private const int SECOND_INDEX = 1;
        public enum Freq
        {
            Daily,
            Weekly,
            Monthly,
            Yearly
        }
        private static Dictionary<Freq, string> freqDescription = new Dictionary<Freq, string>()
            {
                {Freq.Daily,  DAILY},
                {Freq.Weekly,  WEEKLY},
                {Freq.Monthly, MONTHLY},
                {Freq.Yearly, YEARLY}
            };
        private static Dictionary<string, Freq> descFreq = new Dictionary<string, Freq>()
            {
                {DAILY, Freq.Daily},
                {WEEKLY, Freq.Weekly},
                {MONTHLY, Freq.Monthly},
                {YEARLY, Freq.Yearly}
            };
        private Freq freq;
        private int[] cid = new int[DEFAUL_STARTING_LENGTH];
        private DateTime? until = null;


        public Rrule(Freq freq, DateTime until)
        {
            this.freq = freq;
            this.until = until;
        }
        public Rrule(Freq freq, int count = DEFAUL_STARTING_VALUE, int interval = DEFAUL_STARTING_VALUE)
        {

            this.freq = freq;
            this.cid[FIRST_INDEX] = count;
            this.cid[SECOND_INDEX] = interval;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("FREQ=");
            sb.Append(freqDescription.GetValueOrDefault(this.freq));
            if (until != null)
            {
                sb.Append(";UNTIL=");
                sb.Append(until.ToString());
                return sb.ToString();
            }

            if (cid[FIRST_INDEX] != DEFAUL_STARTING_VALUE)
            {
                sb.Append(";COUNT=");
                sb.Append(cid[FIRST_INDEX]);
            }
            if (cid[SECOND_INDEX] != DEFAUL_STARTING_VALUE)
            {
                sb.Append(";INTERVAL=");
                sb.Append(cid[SECOND_INDEX]);
            }
            return sb.ToString();
        }

        public static Rrule fromString(string rule)
        {
            Freq? f = null;
            int count = DEFAUL_STARTING_VALUE, interval = DEFAUL_STARTING_VALUE;
            DateTime? d = null;
            string[] subs = rule.Split(';');
            foreach (string item in subs)
            {
                string[] subsub = item.Split('=');
                if (subsub[FIRST_INDEX] == "FREQ")
                {
                    f = descFreq.GetValueOrDefault(subsub[SECOND_INDEX]);
                }
                else if (subsub[FIRST_INDEX] == "COUNT")
                {
                    count = int.Parse(subsub[SECOND_INDEX]);
                }
                else if (subsub[FIRST_INDEX] == "INTERVAL")
                {
                    interval = int.Parse(subsub[SECOND_INDEX]);
                }
                else
                {
                    d = DateTime.Parse(subsub[SECOND_INDEX]);
                }
            }
            if (d != null)
            {
                return new Rrule((Freq)f, (DateTime)d);
            }
            return new Rrule((Freq)f, count, interval);
        }
    }
}
