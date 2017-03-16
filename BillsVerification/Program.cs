using BillsVerification.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace BillsVerification
{
    class Program
    {
        static void Main(string[] args)
        {
            var lstBills = LoadBills();

            ValidateBills(lstBills);

            PrintValidatedBills(lstBills);

            Console.ReadLine();
        }


        private static void PrintValidatedBills(List<SubmittedBill> bills)
        {
            foreach (var eachBill in bills)
            {
                Console.WriteLine(new String('*', 50));
                Console.WriteLine(string.Format("Emp ID:{0} . Emp Name: {1}", eachBill.Emp.ID, eachBill.Emp.Name));
                foreach (var eachEntry in eachBill.Bills)
                {
                    if (eachEntry.IsValid.HasValue && eachEntry.IsValid.Value)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        foreach (var eachError in eachEntry.ValidationErrors)
                        {
                            Console.WriteLine(eachError);
                        }
                    }
                    Console.WriteLine("ID: {0} ;Bill Amount: {1} ;Bill Date: {2}", eachEntry.ID, eachEntry.Amount, eachEntry.BillDate);
                    Console.ResetColor();
                }
                Console.WriteLine(new String('*', 50));
            }
        }


        private static List<SubmittedBill> LoadBills()
        {
            SubmittedBill[] lstBills = new SubmittedBill[0];
            try
            {
                var billJSON = System.IO.File.ReadAllText("BillsJSON/Bill_1.json");
                lstBills = JsonConvert.DeserializeObject<SubmittedBill[]>(billJSON);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return lstBills.ToList();
        }

        private static void ValidateBills(List<SubmittedBill> bills)
        {
            int i = 0;
            foreach (var eachBill in bills)
            {
                i += eachBill.Bills.Length;
                if (i > 20)
                {
                    System.Threading.Thread.Sleep(60000);
                    i = 0;
                }
                eachBill.Validate();
            }
        }
    }
}
