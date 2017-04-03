using BillsVerification.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidateText;

namespace BillsVerification
{
    public class SubmittedBill
    {
        public Employee Emp { get; set; }

        public ReimbursementEntry Entries { get; set; }

        public Bill[] Bills { get; set; }

        public bool? IsValidated { get; set; }

        public List<string> ValdationErrors { get; set; }


        public void Validate()
        {
            foreach (var eachEntry in this.Bills)
            {
                if (eachEntry.ValidationErrors == null)
                {
                    eachEntry.ValidationErrors = new List<string>();
                }
                try
                {
                    if (System.IO.File.Exists(eachEntry.BillPath))
                    {
                        //eachEntry.IsValid = ComputerVisionHelper.VerifyText(eachEntry.Amount, eachEntry.BillPath, ConfigurationManager.AppSettings["CognitiveServicesKey"], eachEntry.ValueBesideLabel).Result;
                        eachEntry.AmountMatchType = ComputerVisionHelper.VerifyText(eachEntry.Amount, eachEntry.BillPath, ConfigurationManager.AppSettings["CognitiveServicesKey"], eachEntry.ValueBesideLabel).Result;
                    }
                    else
                    {
                        eachEntry.AmountMatchType = MatchType.NoMatch;
                        eachEntry.ValidationErrors.Add("File not found");
                    }
                }
                catch (Exception ex)
                {
                    eachEntry.AmountMatchType = MatchType.NoMatch;
                    eachEntry.ValidationErrors.Add(string.Format("Exception Occured for Bill Id: {0}. Exception Message: {1}", eachEntry.ID, ex.Message));
                }
            }
        }
    }
}
