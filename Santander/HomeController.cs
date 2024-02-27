using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Santander;

[ApiController]
public class HomeController(IMemoryCache memoryCache, IHttpClientFactory clientFactory) : Controller
{
    [HttpGet("[controller]/{n}")]
    public async Task<IActionResult> GetNews(int n)
    {
        if (n <= 0)
        {
            return BadRequest();
        }

        var client = clientFactory.CreateClient(nameof(HomeController));
        if (!memoryCache.TryGetValue("ids", out List<int>? body))
        {
            var idsResponse = await client.GetAsync("/v0/beststories.json");
            body = await idsResponse.Content.ReadFromJsonAsync<List<int>>();
            if (body == null || !body.Any())
            {
                return NotFound();
            }

            memoryCache.Set("ids", body);
        }

        if (n > body.Count)
        {
            return NotFound();
        }

        var stories = new ConcurrentBag<StoryDescription>();
        var tasks = new List<Task>(n);
        Parallel.ForEach(body.Take(n), id =>
        {
            if (memoryCache.TryGetValue(id, out StoryDescription? cachedDescription))
            {
                stories.Add(cachedDescription);
            }
            else
            {
                tasks.Add(Task.Run(async () =>
                {
                    var response = await client.GetAsync($"/v0/item/{id}.json");
                    var description = await response.Content.ReadFromJsonAsync<StoryDescription>();
                    if (description != null)
                    {
                        stories.Add(description);
                        memoryCache.Set(id.ToString(), description);
                    }
                }));
            }
        });

        await Task.WhenAll(tasks);
        var result = stories.ToList().OrderByDescending(x => x.Score).Select(x => new
        {
            title = x.Title,
            uri = x.Url,
            postedBy = x.By,
            time = DateTimeOffset.FromUnixTimeSeconds(x.Time).ToString("yyyy-MM-ddTHH:mm:sszz"),
            score = x.Score,
            commentCount = x.Kids.Length
        });
        return Ok(result);
    }
}