using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SchoolProject_DB.Services
{
    public class TopicScraper
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://jp.finalfantasyxiv.com";

        public TopicScraper()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<(string Title, string ImageUrl, string ArticleUrl)>> ScrapeLatestTopicsAsync()
        {
            var topics = new List<(string Title, string ImageUrl, string ArticleUrl)>();
            string url = $"{BaseUrl}/lodestone/topics/";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return topics;  // 如果請求失敗，返回空列表
                }

                var content = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                // 找到包含新聞列表的 <li>，其中包含 class="news__list--topics ic__topics--list"
                var topicNodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'news__list--topics ic__topics--list')]");

                if (topicNodes != null)
                {
                    // 取最新的 6 條新聞
                    for (int i = 0; i < 9 && i < topicNodes.Count; i++)
                    {
                        var node = topicNodes[i];

                        // 1. 抓取標題 <a>
                        var titleNode = node.SelectSingleNode(".//header[contains(@class, 'news__list--header')]//p[contains(@class, 'news__list--title')]/a");
                        var title = titleNode?.InnerText.Trim();

                        // 2. 抓取圖片 <img>
                        var imgNode = node.SelectSingleNode(".//div[contains(@class, 'news__list--banner')]//a//img");
                        var imgUrl = imgNode?.GetAttributeValue("src", "");

                        // 3. 抓取文章的鏈接 <a href="...">
                        var articleUrl = titleNode?.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(articleUrl) && !articleUrl.StartsWith("http"))
                        {
                            // 將相對鏈接轉換為絕對鏈接
                            articleUrl = $"{BaseUrl}{articleUrl}";
                        }

                        // 確保標題、圖片和文章鏈接都抓取到
                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(imgUrl) && !string.IsNullOrEmpty(articleUrl))
                        {
                            topics.Add((title, imgUrl, articleUrl));
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // 當 HTTP 請求失敗時，記錄錯誤並返回空列表
                Console.WriteLine($"HTTP 請求失敗: {ex.Message}");
                return topics;  // 返回空列表，防止程式崩潰
            }
            catch (Exception ex)
            {
                // 捕捉所有其他異常並記錄錯誤
                Console.WriteLine($"發生錯誤: {ex.Message}");
                return topics;  // 返回空列表
            }

            return topics;
        }

        // 將相對URL轉換為絕對URL的方法
        private string ConvertRelativeUrlsToAbsolute(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 查找所有 <a> 標籤並修正 href 屬性
            var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes != null)
            {
                foreach (var linkNode in linkNodes)
                {
                    var href = linkNode.GetAttributeValue("href", "");
                    if (!href.StartsWith("http"))
                    {
                        // 將相對 URL 轉換為絕對 URL
                        linkNode.SetAttributeValue("href", $"{BaseUrl}{href}");
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }
    }
}
