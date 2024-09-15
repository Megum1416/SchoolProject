using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SchoolProject_DB.ViewComponents
{
    public class VCLevels : ViewComponent
    {
        private readonly string _baseUrl = "https://na.finalfantasyxiv.com/lodestone/character/";

        public async Task<IViewComponentResult> InvokeAsync(string lodestoneID)
        {
            // 動態生成完整的網址，將使用者B的lodestoneID帶入URL
            var url = $"{_baseUrl}{lodestoneID}/class_job/";

            // 爬取到的 22 個職業等級（或符號 '-'）將存入 List 中
            var levels = await ScrapeLevels(url);

            // 如果未抓取到資料或資料不完整，則全部顯示 "-"
            if (levels.Count != 22)
            {
                levels = new List<string>(new string[22]);  // 建立22個"-"
                for (int i = 0; i < 22; i++)
                {
                    levels[i] = "-";
                }
            }

            return View(levels);  // 將等級數據傳遞給 View
        }

        private async Task<List<string>> ScrapeLevels(string url)
        {
            var levels = new List<string>();  // 使用 List 來儲存職業等級

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetStringAsync(url);
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);

                    // 抓取 class = 'character__job__level' 的節點
                    var jobLevelNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='character__job__level']");

                    if (jobLevelNodes != null)
                    {
                        int count = 0;

                        foreach (var jobLevelNode in jobLevelNodes)
                        {
                            if (count >= 22) break;  // 只抓取前 22 個職業等級

                            // 抓取職業等級或符號 '-'
                            string jobLevel = jobLevelNode.InnerText.Trim();  // 取得等級數值或 '-'
                            levels.Add(jobLevel);  // 加入 List 中
                            count++;  // 計數器遞增
                        }
                    }
                }
            }
            catch
            {
                // 發生錯誤時，直接返回空列表，InvokeAsync會處理
                return new List<string>();
            }

            return levels;
        }
    }
}
