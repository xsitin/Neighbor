using System.Net.Mime;
using System.Text.Json;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

using Common.Data;

[ApiController]
[Route("ads")]
public class AdvertisementsController : Controller
{
    private IAdRepository AdRepository { get; init; }

    private IImageRepository ImageRepository { get; init; }
    private ILogger<AdvertisementsController> Logger { get; init; }

    public AdvertisementsController(AdRepository adRepository,
        IImageRepository imageRepository, ILogger<AdvertisementsController> logger)
    {
        AdRepository = adRepository;
        ImageRepository = imageRepository;
        Logger = logger;
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetRecommended([FromQuery] int page = 1, [FromQuery] int pageSize = 21)
    {
        var request = new AdRequest(DataRequestTypes.Popular,
            paginationInfo: new PaginationInfo<Ad> {Page = page, PageSize = pageSize});
        return new ContentResult()
        {
            Content = JsonSerializer.Serialize(await AdRepository.GetPage(request)), ContentType = "text/json"
        };
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 21)
    {
        var ads = await AdRepository.GetPage(
            new AdRequest(
                DataRequestTypes.Search, query: query,
                paginationInfo: new PaginationInfo<Ad>() {Page = page, PageSize = pageSize}));
        return Json(ads);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Category([FromQuery] string category, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 21)
    {
        var ads = await AdRepository.GetPage(
            new AdRequest(
                DataRequestTypes.FromCategory, category: category,
                paginationInfo: new PaginationInfo<Ad> {Page = page, PageSize = pageSize}));
        return Json(ads);
    }

    [Authorize]
    [HttpGet("getUserAds")]
    public async Task<IActionResult> GetUserAds([FromQuery] int page = 1, [FromQuery] int pageSize = 21)
    {
        var name = User.FindFirst(nameof(Account.Name))?.Value;
        var ads = await AdRepository.GetPage(
            new AdRequest(
                DataRequestTypes.FromUser, username: name,
                paginationInfo: new PaginationInfo<Ad> {Page = page, PageSize = pageSize}));
        return Json(ads);
    }

    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var ad = await AdRepository.GetById(id);
        return Json(ad);
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> LoadAd()
    {
        var ad = JsonSerializer.Deserialize<Ad>(Request.Form[nameof(Ad)].ToString());
        var username = User.Claims.First(x => x.Type == nameof(Account.Name)).Value;
        if (username != ad.OwnerName && !User.IsInRole("Administrator"))
            return Forbid();
        var images = Request.Form.Files;
        var saveTasks = ImageRepository.AddAll(images).ToArray();
        Task.WaitAll(saveTasks);
        var ids = saveTasks.Select(x => x.Result.Id).ToArray();
        ad.ImagesIds = ids;
        await AdRepository.Create(ad);
        return Ok();
    }

    [DisableCors]
    [HttpGet("/img/get/{imageId}")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60 * 5)]
    public async Task<IActionResult> GetImage(string imageId)
    {
        var image = ImageRepository.GetContent(imageId);
        return new FileContentResult(image, MediaTypeNames.Image.Jpeg);
    }

    [Authorize]
    [HttpPatch("update")]
    public async Task<IActionResult> Update()
    {
        var receivedAd = JsonSerializer.Deserialize<Ad>(Request.Form[nameof(Ad)].ToString());
        var username = User.Claims.First(x => x.Type == nameof(Account.Name)).Value;
        if ((username != receivedAd.OwnerName) && !User.IsInRole("Administrator"))
            return Forbid();
        var images = Request.Form.Files;
        receivedAd.ImagesIds = ImageRepository.AddAll(images).Select(x => x.Result.Id).ToArray();

        await AdRepository.Update(receivedAd);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id}/delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var username = User.Claims.First(x => x.Type == nameof(Account.Name)).Value;
        var ad = await AdRepository.GetById(id);
        if (username != ad.OwnerName && !User.IsInRole("Administrator"))
            return Forbid();
        await AdRepository.Delete(ad);
        return Ok();
    }
}
