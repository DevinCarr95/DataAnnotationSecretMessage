using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataAnnotationSecretMessage
{
    internal class MessageDecoder
    {
        public static async Task Main(string[] args)
        {
            string url = "https://docs.google.com/document/d/e/2PACX-1vRPzbNQcx5UriHSbZ-9vmsTow_R6RRe7eyAU60xIF9Dlz-vaHiHNO2TKgDi7jy4ZpTpNqM7EvEcfr_p/pub";

            await DecodeSecretMessage(url);
        }

        public static async Task DecodeSecretMessage(string url)
        {
            using HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(url);

            var lines = response.Substring(response.IndexOf("<tr") + 1);

            var graphLines = lines.Split('\n');
            graphLines = graphLines[0].Split("<tr");

            var messageData = ExtractData(graphLines);

            PrintSecretMessage(messageData);
        }

        public static List<KeyValuePair<int, List<KeyValuePair<(int, int), string>>>> ExtractData(string[] graphLines)
        {
            var gridValues = new Dictionary<(int, int), string>();

            foreach (var line in graphLines)
            {
                var matches = Regex.Matches(line, @"<span class=""c."">(.*?)</span>");

                int x = 0;
                int y = 0;
                string character = matches[1].Groups[1].Value;

                try
                {
                    x = (Int32.Parse(matches[0].Groups[1].Value));
                    y = (Int32.Parse(matches[2].Groups[1].Value));
                }
                catch (Exception ex)
                {
                    
                }

                gridValues[(x, y)] = character;
            }

            var sortedValues = gridValues
            .GroupBy(kvp => kvp.Key.Item1)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(kvp => kvp.Key.Item2).ToList()
            ).OrderBy(kvp => kvp.Key).ToList();

            return sortedValues;
        }

        public static void PrintSecretMessage(List<KeyValuePair<int, List<KeyValuePair<(int, int), string>>>> messageData)
        {
            int column = 0;
            int maxColumnWidth = 7;

            for (int i = 0; i < messageData.Count; i++)
            {
                for (int x = 0; x <= messageData[i].Value.Count - 1;)
                {
                    var entry = messageData[i].Value[x];
                    
                    if (messageData[i].Value[x].Key.Item2 > column)
                    {
                        Console.Write(' ');
                        column++;
                    }
                    else
                    {
                        Console.Write(messageData[i].Value[x].Value);
                        column++;
                        x++;
                    }
                }

                if (column < maxColumnWidth)
                {
                    string remainingSpaces = new string(' ', maxColumnWidth - column);
                    Console.Write(remainingSpaces);
                }

                column = 0;

                Console.Write('\n');
            }
        }
    }
}
