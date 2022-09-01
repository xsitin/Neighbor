using System.Net.Mime;
using System.Text.Json;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WebApi.Infrastructure;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("ads")]
    public class AdvertismentsController : Controller
    {
        private AdRepository adRepository { get; set; }

        private IImageRepository imageRepository { get; set; }
        private IRatingCalculator RatingCalculator { get; set; }
        private ILogger<AdvertismentsController> Logger { get; set; }

        public AdvertismentsController(AdRepository adRepository, IRatingCalculator ratingCalculator,
            IImageRepository imageRepository, ILogger<AdvertismentsController> logger)
        {
            this.adRepository = adRepository;
            RatingCalculator = ratingCalculator;
            this.imageRepository = imageRepository;
            Logger = logger;
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetRecommended([FromQuery] int page = 1, [FromQuery] int pageSize = 21)
        {
            return new ContentResult()
            {
                Content = JsonSerializer.Serialize(await adRepository.GetRecommendedAsync(page, pageSize)),
                ContentType = "text/json"
            };
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int page = 1,
            [FromQuery] int pageSize = 21)
        {
            var ads = await adRepository.GetSimilar(query, page, pageSize);
            foreach (var ad in ads) ad.SId = ad.Id.ToString();

            return Json(ads);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> Category([FromQuery] string category, int page = 1, int pageSize = 21)
        {
            var ads = await adRepository.GetWithCategory(category, page, pageSize);
            return Json(ads);
        }

        [Authorize]
        [HttpGet("getUserAds")]
        public async Task<IActionResult> GetUserAds()
        {
            var name = User.FindFirst(nameof(Account.Name))?.Value;
            return Json(await adRepository.GetByUserNameAsync(name));
        }

        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var ad = await adRepository.GetByIdAsync(ObjectId.Parse(id));
            return Json(ad);
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> LoadAd()
        {
            var adJson = Request.Form[nameof(Ad)];
            var ad = JsonSerializer.Deserialize<Ad>(adJson.ToString());
            var username = User.Claims.First(x => x.Type == nameof(Account.Name)).Value;
            if (username != ad.OwnerName && !User.IsInRole("Administrator"))
                return Forbid();
            var images = Request.Form.Files;
            await adRepository.AddAsync(ad, images);
            return Ok();
        }

        [DisableCors]
        [HttpGet("/img/get/{imageId}")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60 * 5)]
        public IActionResult GetImage(string imageId)
        {
            var image = imageRepository.Get(imageId);
            return new FileContentResult(image, MediaTypeNames.Image.Jpeg);
        }

        [Authorize]
        [HttpPatch("update")]
        public async Task<IActionResult> Update()
        {
            var adJson = Request.Form[nameof(Ad)];
            var ad = JsonSerializer.Deserialize<Ad>(adJson.ToString());
            var username = User.Claims.First(x => x.Type == nameof(Account.Name)).Value;
            if ((username != ad.OwnerName) && !User.IsInRole("Administrator"))
                return Forbid();
            var images = Request.Form.Files;
            await adRepository.UpdateAsync(ad, images);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var username = User.Claims.First(x => x.Type == nameof(Account.Name)).Value;
            var objId = ObjectId.Parse(id);
            var ad = await adRepository.GetByIdAsync(objId);
            if (username != ad.OwnerName && !User.IsInRole("Administrator"))
                return Forbid();
            await adRepository.DeleteAsync(objId);
            return Ok();
        }
    }
}