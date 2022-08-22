using Chase.Clap.OSINT.VirusTotal;
using System;
using System.Net.Http;

namespace OsIntMiner
{
    class Program
    {
        static void Main(string[] args)
        {
            KeyRing keys = new KeyRing(new string[] {
                "<key1>",
                "<key2>",
                "<key3>",
                "<key4>",
                "<key5>"
            });
            RestClient cl = new RestClient(keys);

            HttpResponseMessage resp = cl.MakeRequest("https://www.twitch.tv");

            Console.WriteLine(resp.Content.ReadAsStringAsync().GetAwaiter().GetResult());


            Console.WriteLine("Hello World!");
        }
    }
}
