using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using ProductsFeeder.Models;
using System.IO;
using System.Net;
namespace ProductsFeeder
{
    static class Program
    {

        /// <summary>
        /// Start Function
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
       
            Console.WriteLine("We have start the Application\n 1. we are going to download the file data."+DateTime.Now.ToString());
            var CSVTeapplixData = GetOrdersFromTeapplixRemote();
            Console.WriteLine("2. We are going to Populated the Entity."+DateTime.Now.ToString());
            var Orders = LoadOrdersFromTeapplixFile(CSVTeapplixData);
            Console.WriteLine("3. Now we are going to update the table data." + DateTime.Now.ToString());
            SaveProducts(Orders);
            Console.WriteLine("4. Table is updated on " + DateTime.Now.ToString());
        }        
      /// <summary>
      /// In this method, we will download the csv and read it.
      /// </summary>
      /// <param name="CSVteaplixData"></param>
      /// <returns></returns>
        private static List<Order> LoadOrdersFromTeapplixFile(string CSVteaplixData)
        {
            // DataTable csvData = new DataTable();
            List<Order> listOrder = new List<Order>();

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(new StringReader(CSVteaplixData)))
                {

                    csvReader.HasFieldsEnclosedInQuotes = true;
                    csvReader.Delimiters = new string[] { "," };
                   
                    string[] colFields = csvReader.ReadFields();
                    
                    while (!csvReader.EndOfData)
                    {
                        try
                        {

                           
                            string[] dataRow = csvReader.ReadFields();
                            var Order = new Order();
                            IFormatProvider culture = new CultureInfo("en-US", true);
                            Order.order_source = dataRow[0].ToString();
                            Order.account = dataRow[1].ToString();
                            Order.txn_id= dataRow[2].ToString();
                            Order.date = Convert.ToDateTime(dataRow[5]);
                            Order.Datetest = Convert.ToString(Order.date);
                            Order.status=Convert.ToString(dataRow[6]);
                            Order.name=Convert.ToString(dataRow[9]);
                            Order.payer_email=Convert.ToString(dataRow[10]);
                            Order.address_city=Convert.ToString(dataRow[15]);
                            Order.address_country=Convert.ToString(dataRow[12]);
                            Order.address_state=Convert.ToString(dataRow[13]);
                            Order.address_street=Convert.ToString(dataRow[16]);
                            Order.address_street2=Convert.ToString(dataRow[17]);
                            Order.address_zip=Convert.ToString(dataRow[14]);
                            Order.tracking=Convert.ToString(dataRow[27]);
                            Order.item_name=Convert.ToString(dataRow[32]);
                            Order.item_sku=Convert.ToString(dataRow[34]);
                            Order.item_description = dataRow[39].ToString();
                            listOrder.Add(Order); 
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to read a csv record: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return listOrder;
        }

        /// <summary>
        /// IN this method, we are going to save the new record in the database on the bases of date
        /// </summary>
        /// <param name="newProductsList"></param>
        private static void SaveProducts(List<Order> newProductsList)
        {
            using (OrderEntities db = new OrderEntities())
            {
                var Orders = db.Orders.ToList();

                var result = newProductsList.Where(p => !Orders.Any(p2 => p2.Datetest == p.Datetest)).ToList();
                //var result = newProductsList.Where(p => Orders.Any(p2 => Convert.ToDateTime(p2.date).ToShortDateString() == Convert.ToDateTime(p.date).ToShortDateString())).ToList();
                Console.WriteLine("No of new records in the Csv file " + result.Count.ToString());
                if (result.Count > 0)
                {
                    db.Orders.AddRange(result);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// In this method, we have read the csv value and convert to List<Order> so after that we can save it.
        /// </summary>
        /// <returns></returns>
        public static string GetOrdersFromTeapplixRemote()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = ConfigurationManager.AppSettings["TeapplixRemote"].ToString();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader sr = new StreamReader(response.GetResponseStream());
                string csvResults = sr.ReadToEnd();
                sr.Close();

                return csvResults;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;
            }
        }
    }
  
}


