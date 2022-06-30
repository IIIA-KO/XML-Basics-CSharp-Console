using System.Xml;
using System.Text.RegularExpressions;

Console.OutputEncoding = System.Text.Encoding.Unicode;
Console.InputEncoding = System.Text.Encoding.Unicode;

try
{
    Task1();
    //Task2();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(ex.Message);
    Console.ResetColor();
}

static void Task1()
{
    //Я пішов шляхом костилів, але воно працює)

    string HtmlFileName = "Курс_валют.html";
    string XmlFileName = "output.xml";

    string[] HtmlFile = File.ReadAllLines(HtmlFileName);

    string html = string.Join("\n", HtmlFile);

    int index1 = html.IndexOf("<tbody class=\"bank_rates_eur\"");
    html = html.Substring(0, index1);

    int index2 = html.IndexOf("<tbody class=\"bank_rates_usd\"");
    html = html.Substring(index2);


    List<int> startInd = html.AllIndexesOf("<tr");
    List<int> endInd = html.AllIndexesOf("</tr>");

    List<string> strings = new();
    for (int i = 0; i < startInd.Count; i++)
        strings.Add(html.Substring(startInd[i], endInd[i] - startInd[i]));


    string pattern1 = @"\<[^>]+\>";
    Regex regex1 = new Regex(pattern1, RegexOptions.Multiline);
    for (int i = 0; i < strings.Count; i++)
    {
        MatchCollection matches1 = regex1.Matches(strings[i]);
        for (int j = 0; j < matches1.Count; j++)
            strings[i] = strings[i].Replace(matches1[j].ToString(), "*");
    }

    List<RateUSD> RateUSD = new();
    for (int i = 0; i < strings.Count; i++)
    {
        var data = strings[i]
            .Split('*')
            .ToList();

        data.RemoveAll(s => string.IsNullOrWhiteSpace(s));

        for (int j = 1; j < data.Count; j += 3)
            data.RemoveAt(j);

        RateUSD tmp;
        List<string> names = new();
        List<decimal> buyRates = new();
        List<decimal> sellRates = new();
        decimal buy, sell;

        for (int j = 0; j < data.Count; j += 3)
            names.Add(data[j]);

        for (int j = 1; j < data.Count; j += 3)
        {
            data[j] = data[j].Replace('.', ',');
            buy = Convert.ToDecimal(data[j]);
            buyRates.Add(buy);
        }

        for (int j = 2; j < data.Count; j += 3)
        {
            data[j] = data[j].Replace('.', ',');
            sell = Convert.ToDecimal(data[j]);
            sellRates.Add(sell);
        }

        for (int j = 0; j < names.Count; j++)
        {
            tmp = new(names[j], buyRates[j], sellRates[j]);
            RateUSD.Add(tmp);
        }

    }
    foreach (var rate in RateUSD)
        Console.WriteLine(rate);

    WriteRatesUSD(XmlFileName, RateUSD);
}
static void Task2()
{
    Random r = new Random();
    List<Order> orders = new List<Order>();
    for (int i = 0; i < r.Next(3, 10); i++)
    {
        orders.Add(new Order());
        orders[i].Print();
        Console.WriteLine(new String('-', 50));
    }
    WriteOrders("ORDERS.xml", orders);
}

static void WriteOrders(string filename, List<Order> orders)
{
    XmlTextWriter? writer = null;
    try
    {
        writer = new XmlTextWriter(filename, System.Text.Encoding.UTF8);

        writer.Formatting = Formatting.Indented;

        writer.WriteStartDocument();
        {
            writer.WriteStartElement("orders");
            {
                foreach (Order order in orders)
                {

                    writer.WriteStartElement("order");
                    {

                        writer.WriteStartElement("products");
                        {

                            foreach (var item in order.products)
                            {

                                writer.WriteStartElement("product");
                                {

                                    writer.WriteElementString("name", item.Name);
                                    writer.WriteElementString("price", item.Price.ToString());
                                    writer.WriteElementString("shelflife", item.ShelfLife.ToShortDateString());

                                }
                                writer.WriteEndElement();

                            }

                        }
                        writer.WriteEndElement();

                    }
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();

        }
        writer.WriteEndDocument();

        Console.WriteLine("Замовлення успішно збережено " + filename);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ResetColor();
    }
    finally
    {
        writer?.Close();
    }
}
static void WriteRatesUSD(string filename, List<RateUSD> RateUSD)
{
    XmlTextWriter? writer = null;
    try
    {
        writer = new XmlTextWriter(filename, System.Text.Encoding.UTF8);

        writer.Formatting = Formatting.Indented;

        writer.WriteStartDocument();
        {
            writer.WriteStartElement("rates_usd");
            {
                foreach (RateUSD order in RateUSD)
                {

                    writer.WriteStartElement("rate");
                    {

                        foreach (var item in RateUSD)
                        {

                            writer.WriteStartElement("product");
                            {

                                writer.WriteElementString("name", item.BankName);
                                writer.WriteElementString("buy_rate", item.BuyRate.ToString());
                                writer.WriteElementString("sell_rate", item.SellRate.ToString());

                            }
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
        writer.WriteEndDocument();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ResetColor();
    }
    finally
    {
        writer?.Close();
    }
}

public static class StringExtention
{
    public static List<int> AllIndexesOf(this string str, string value)
    {
        if (String.IsNullOrEmpty(value))
            throw new ArgumentException("the string to find may not be empty", "value");
        List<int> indexes = new List<int>();
        for (int index = 0; ; index += value.Length)
        {
            index = str.IndexOf(value, index);
            if (index == -1)
                return indexes;
            indexes.Add(index);
        }
    }
}
public class Order
{
    public List<Product> products;
    private Random random;

    public Order()
    {
        random = new Random();

        products = new();
        for (int i = 0; i < random.Next(5, 15); i++) products.Add(new());
    }

    public void Print()
    {
        for (int i = 0; i < products.Count; i++) Console.WriteLine(products[i]);
    }
}
public class Product
{
    public string Name { get; set; }
    public double Price { get; set; }
    public DateTime ShelfLife = new();

    private Random random;

    private string[] Names = { "Хліб", "Молоко", "Борошно", "Масло", "Гречка", "Олія", "Цукерки", "Чай", "Кава", "Яйця", "М'ясо" };
    public Product()
    {
        random = new Random();
        Name = Names[random.Next(Names.Length)];
        Price = random.Next(10, 150);
        ShelfLife = GenRandomDate(DateTime.Now.AddDays(random.Next(-100, 1)), DateTime.Today.AddDays(random.Next(1, 100)), random);
    }
    public override string ToString() => $"| {Name,-10} | {Price,-5} | {ShelfLife:dd-MM-yyyy} |";

    private static DateTime GenRandomDate(DateTime from, DateTime to, Random random = null)
    {
        if (from >= to)
            throw new Exception("The parameter \"from\" must be smaller than the parameter \"to\"!");

        if (random == null)
            random = new Random();

        int daysDiff = (to - from).Days;
        return from.AddDays(random.Next(daysDiff));
    }
}
public class RateUSD
{
    public string BankName { get; private set; }
    public decimal BuyRate { get; private set; }
    public decimal SellRate { get; private set; }

    public RateUSD(string bankName, decimal buyRate, decimal sellRate)
    {
        BankName = bankName;
        BuyRate = buyRate;
        SellRate = sellRate;
    }

    public override string ToString() => $"| {BankName,-30} | {BuyRate,-10} | {SellRate,-10} |";
}