using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AngleSharp;
using Service.Dtos;
using Service.Interfaces;

namespace Service.Implements
{
    public class NbaPlayerService : INbaPlayerService
    {
        /// <summary>
        /// 取得Nba球員生涯資料壓縮檔
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetAsync()
        {
            var nbaPlayerCareer = await this.GetNbaPlayerData();
            var csvZip = await GenerateExportZip(nbaPlayerCareer);
            return csvZip.ToArray();
        }

        /// <summary>
        /// 建立匯出的 zip 檔案
        /// </summary>
        /// <param name="nbaPlayers"></param>
        /// <returns></returns>
        private static async Task<MemoryStream> GenerateExportZip(List<NbaPlayer> nbaPlayers)
        {
            var memoryStream = new MemoryStream();
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);

            foreach (var group in nbaPlayers.GroupBy(x => x.NameType))
            {
                var csvFileName = archive.CreateEntry(Path.Combine(group.Key + ".csv"));

                var title = "Player,G,PTS,TRB,AST,FG(%),FG3(%),FT(%),eFG(%),PER,WS";

                await using var entryStream = csvFileName.Open();
                await using var streamWriter = new StreamWriter(entryStream);
                await streamWriter.WriteLineAsync(title);
                foreach (var item in group.OrderBy(x => x.Career.Player))
                {
                    await streamWriter.WriteLineAsync(CsvGenerator(item.Career));
                }
            }

            return memoryStream;
        }

        /// <summary>
        /// 轉換Csv逗號分隔字串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private static string CsvGenerator<T>(T data)
        {
            var t = typeof(T);
            var propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var csvLine = string.Join(",", propInfos.Select(i => i.GetValue(data)));
            return csvLine;
        }

        /// <summary>
        /// 取得Nba球員生涯資料
        /// </summary>
        /// <returns></returns>
        private async Task<List<NbaPlayer>> GetNbaPlayerData()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync("https://www.basketball-reference.com/players/");
            var contents = document.QuerySelectorAll(".page_index li");
            var children = contents.Select(x => x.Children);
            var nbaPlayers = new List<NbaPlayer>();

            foreach (var child in children)
            {
                if (!child.Any())
                {
                    continue;
                }

                var playerInfo = from element in child.Last().Children
                                 select new { href = element.GetAttribute("Href"), Player = element.TextContent };

                Parallel.ForEach(playerInfo, info =>
                {
                    var nbaPlayer = new NbaPlayer { NameType = child.First().InnerHtml };

                    var doc = context.OpenAsync("https://www.basketball-reference.com" + info.href).Result;

                    var content = doc.QuerySelectorAll(".stats_pullout div div");
                    var player = from careers in content.Select(x => x.Children)
                                 select new { career = careers.Select(x => x.TextContent) };

                    if (player.Any())
                    {
                        var career = new Career();
                        var type = typeof(Career).GetProperties();
                        career.Player = info.Player;
                        foreach (var item in type)
                        {
                            var data = player.Where(x => x.career.First().Replace("%", "") == item.Name);
                            if (data.Any())
                            {
                                item.SetValue(career, data.First().career.Last());
                            }
                        }

                        nbaPlayer.Career = career;
                    }

                    nbaPlayers.Add(nbaPlayer);
                });
            }

            return nbaPlayers;
        }
    }
}