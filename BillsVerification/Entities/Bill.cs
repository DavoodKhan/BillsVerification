using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidateText;

namespace BillsVerification.Entities
{
    //public enum MatchType
    //{
    //        FullMatch,
    //        NoMatch,
    //        PartialMatch
    //}

    public class Bill
    {
        public int ID { get; set; }

        public double Amount { get; set; }

        public string BillPath { get; set; }

        public DateTime BillDate { get; set; }

        //public bool? IsValid { get; set; }

        public MatchType? AmountMatchType { get; set; }

        public bool ValueBesideLabel { get; set; }

        public List<string> ValidationErrors { get; set; }
    }
}
