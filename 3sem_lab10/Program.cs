using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.Net.Http;

namespace _3sem_lab10
{
    public class DBContext : DbContext
    {
        public DBContext() : base("name=DBContext")
        {
        }

        public DbSet<Stock> Stocks { get; set; }
    }
    public class Stock
    {
        public int Id { get; set; }
        public string Ticker { get; set; }
        public decimal PriceToday { get; set; }
        public decimal PriceYesterday { get; set; }
    }
    class Program
    {
        static async Task Main()
        {
            List<string> tickers = ReadTickersFromFile("ticker.txt");

            List<Task> tasks = new List<Task>();

            using (var dbContext = new DBContext())
            {
                foreach (string ticker in tickers)
                {
                    Task task = ProcessStockAsync(ticker, dbContext);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                Console.WriteLine("All tasks completed.");
            }
            Console.Write("Введите тикер: ");
            string userTicker = Console.ReadLine();

            using (var dbContext = new DBContext())
            {
                var stockData = dbContext.Stocks
                    .Where(s => s.Ticker == userTicker)
                    .OrderByDescending(s => s.Id)
                    .Take(2)
                    .ToList();

                if (stockData.Count == 2)
                {
                    double priceToday = (double)stockData[0].PriceToday;
                    double priceYesterday = (double)stockData[1].PriceYesterday;

                    string changeStatus = GetChangeStatus(priceYesterday, priceToday);

                    Console.WriteLine($"Цена за вчерашний день: {priceYesterday}");
                    Console.WriteLine($"Цена за сегодня: {priceToday}");
                    Console.WriteLine($"Изменение цены: {changeStatus}");
                }
                else
                {
                    Console.WriteLine($"Недостаточно данных для тикера {userTicker}");
                }
            }

            Console.ReadLine();
        }
        private static readonly object fileLock = new object();
        static string GetChangeStatus(double yesterday, double today)
        {
            const double epsilon = 0.0001;

            if (Math.Abs(today - yesterday) < epsilon)
            {
                return "Цена осталась неизменной";
            }
            else if (today > yesterday)
            {
                return "Цена выросла";
            }
            else
            {
                return "Цена упала";
            }
        }
        static List<string> ReadTickersFromFile(string filePath)
        {
            return File.ReadAllLines(filePath).ToList();
        }

        static double CalculateAveragePrice(string csvData)
        {
            string[] lines = csvData.Split('\n');
            double sum = 0;
            int count = 0;

            foreach (string line in lines)
            {
                if (line[0] == 'D') continue;
                else
                {
                    string[] fields = line.Split(',');
                    if (fields.Length >= 5 && fields[2] != null && fields[3] != null)
                    {
                        try
                        {
                            double high = Convert.ToDouble(fields[2].Replace('.', ','));
                            double low = Convert.ToDouble(fields[3].Replace('.', ','));
                            sum += (high + low) / 2;
                            count++;
                        }
                        catch { return 0.0; }
                    }
                    else { return 0.0; }
                }
            }

            return count > 0 ? sum / count : 0.0;
        }



        static async Task ProcessStockAsync(string ticker, DBContext dbContext)
        {
            string apiUrl = $"https://query1.finance.yahoo.com/v7/finance/download/{ticker}?period1={GetUnixTimestampLastYear()}&period2={GetUnixTimestampNow()}&interval=1d&events=history&includeAdjustedClose=true";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string csvData = await client.GetStringAsync(apiUrl);
                    double averagePrice = CalculateAveragePrice(csvData);

                    Stock stock = new Stock
                    {
                        Ticker = ticker,
                        PriceToday = (decimal)averagePrice,
                        PriceYesterday = (decimal)GetYesterdayPrice(csvData)
                    };

                    lock (fileLock)
                    {
                        dbContext.Stocks.Add(stock);
                        dbContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {ticker}: {ex.Message}");
                }
            }
        }
        static void WriteResultToFile(string result)
        {
            File.AppendAllText("result.txt", result + Environment.NewLine);
        }

        static long GetUnixTimestampNow()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        static long GetUnixTimestampLastYear()
        {
            return DateTimeOffset.Now.AddYears(-1).ToUnixTimeSeconds();
        }

        static double GetYesterdayPrice(string csvData)
        {
            string[] lines = csvData.Split('\n');

            if (lines.Length >= 3)
            {
                string[] fields = lines[1].Split(',');

                if (fields.Length >= 5 && fields[2] != null && fields[3] != null)
                {
                    try
                    {
                        double high = Convert.ToDouble(fields[2].Replace('.', ','));
                        double low = Convert.ToDouble(fields[3].Replace('.', ','));
                        return (high + low) / 2;
                    }
                    catch
                    {
                        return 0.0;
                    }
                }
            }

            return 0.0;
        }
    }
}
