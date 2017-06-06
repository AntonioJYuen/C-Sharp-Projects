using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Collections;

namespace ImportFXRates {

    public class rate {
        public String symbol { get; set; }
        public Double price { get; set; }
        public DateTime date { get; set; }
    }

    class Program {
        static void Main(string[] args) {

            sqlInsert();

        }

        static void sqlInsert() {
            

            string connectionString = "Data Source=NJSQLIMIS152; Initial Catalog=PRODIMIS; User ID=fxImport; Password=fxImport";

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {

                _conn.Open();

                foreach (rate r in getRates())
                {
                    try
                    {
                        string queryStatement = "INSERT INTO fxRates ";
                        queryStatement += "VALUES (@symbol, @price, @date)";

                        SqlCommand cmd = new SqlCommand(queryStatement, _conn);
                        cmd.Parameters.AddWithValue("@symbol", r.symbol);
                        cmd.Parameters.AddWithValue("@price", r.price);
                        cmd.Parameters.AddWithValue("@date", r.date);

                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        Console.WriteLine(DateTime.Now + ": " + "Duplicate Entry :" + r.symbol);
                    }
                }

                if (_conn.State != ConnectionState.Closed)
                {
                    _conn.Close();
                }

            }

        }

        static List<rate> getRates() {

            List<rate> rateList = new List<rate>();

            String URLString = "https://finance.yahoo.com/webservice/v1/symbols/allcurrencies/quote";

            XmlDocument doc = new XmlDocument();
            doc.Load(URLString);

            XmlNodeList quoteList = doc.GetElementsByTagName("resource");
            for (int i = 0; i < quoteList.Count; i++)
            {
                XmlNodeList fieldList = quoteList[i].ChildNodes;

                int j = 0;
                int price = 0;
                int symbol = 0;
                int utctime = 0;

                foreach(XmlNode node in fieldList)
                {
                    
                    if (node.Attributes["name"].Value.ToString().Equals("price"))
                    {
                        price = j;
                    }

                    if (node.Attributes["name"].Value.ToString().Equals("symbol"))
                    {
                        symbol = j;
                    }

                    if (node.Attributes["name"].Value.ToString().Equals("utctime"))
                    {
                        utctime = j;
                    }

                    j++;

                }

                if (Double.Parse(fieldList[1].InnerText) != 0) {
                    
                    try
                    {
                        rate data = new rate();
                        data.symbol = fieldList[symbol].InnerText.ToString().Substring(0, 3);
                        data.price = Math.Round(1d / Double.Parse(fieldList[price].InnerText.ToString()), 6);
                        data.date = DateTime.Parse(fieldList[utctime].InnerText);

                        rateList.Add(data);
                    }
                    catch
                    {
                        Console.WriteLine(DateTime.Now + ": " + "Failed on adding:    " + fieldList[symbol].InnerText.ToString().Substring(0, 3));
                    }
                }
                else
                {
                    Console.WriteLine(DateTime.Now + ": " + "Failed to read:  " + DateTime.Today + "    |   " + fieldList[symbol].InnerText.ToString().Substring(0, 3));
                }

            }

            return rateList;

        }
    }
}
