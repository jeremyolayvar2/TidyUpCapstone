using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Models.DTOs;
using TidyUpCapstone.Models.Entities;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Models.ViewModels;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        //private readonly ILogger<ItemController> _logger;
        //private readonly IItemPostService _itemPostService;
        //private readonly IUserService _userService;
        //private readonly UserManager<AppUser> _userManager;

        //public ItemController(
        //    ILogger<ItemController> logger,
        //    IItemPostService itemPostService,
        //    IUserService userService,
        //    UserManager<AppUser> userManager)
        //{
        //    _logger = logger;
        //    _itemPostService = itemPostService;
        //    _userService = userService;
        //    _userManager = userManager;
        //}

        //[HttpPost]
        //[IgnoreAntiforgeryToken]
        //public async Task<IActionResult> Create([FromForm] ItemPostDto itemPostDto)
        //{
        //    _logger.LogInformation("Creating item: {Title}", itemPostDto.ItemTitle);

        //    if (itemPostDto == null)
        //        return BadRequest("Invalid item data");

        //    if (itemPostDto.ImageFile == null)
        //        ModelState.AddModelError("ImageFile", "The image file is required");

        //    var appUser = await _userManager.GetUserAsync(User);
        //    if (appUser == null)
        //        return RedirectToAction("Login", "Account");

        //    if (string.IsNullOrWhiteSpace(itemPostDto.LocationName))
        //        ModelState.AddModelError("LocationName", "Location is required.");

        //    if (!ModelState.IsValid)
        //    {
        //        var items = await _itemPostService.GetAllItemPostsAsync();
        //        var tokenBalance = await _userService.GetUserTokenBalanceAsync(appUser.Id);
        //        return View("~/Views/Home/Main.cshtml", new MainPageViewModel
        //        {
        //            ItemPosts = items,
        //            NewItemPost = itemPostDto,
        //            CurrentUserTokenBalance = tokenBalance
        //        });
        //    }

        //    try
        //    {
        //        var location = await _itemPostService.ResolveOrCreateLocationAsync(itemPostDto.LocationName);
        //        var createdItem = await _itemPostService.CreateItemPostAsync(itemPostDto, appUser.Id, location.Id);

        //        _logger.LogInformation("Item created: {ItemId}", createdItem.Id);
        //        TempData["SuccessMessage"] = "Item posted successfully!";
        //        return RedirectToAction("Main", "Home");
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        _logger.LogError(dbEx, "DB error");
        //        return StatusCode(500, new { message = "Internal Server Error", details = dbEx.InnerException?.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating item");
        //        return StatusCode(500, new { message = "Internal Server Error", details = ex.Message });
        //    }
        //}

        //[HttpGet]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    try
        //    {
        //        var item = await _itemPostService.GetItemPostByIdAsync(id);
        //        var currentUser = await _userManager.GetUserAsync(User);

        //        if (item == null)
        //            return NotFound();

        //        if (currentUser == null || item.UserId != currentUser.Id)
        //            return Forbid();

        //        return Json(new
        //        {
        //            itemTitle = item.ItemTitle,
        //            itemCategory = item.Category?.Name,
        //            itemCondition = item.Condition?.Name,
        //            itemLocation = item.Location?.Name,
        //            itemTokenPrice = item.AdjustedTokenPrice,
        //            finalTokenPrice = item.FinalTokenPrice,
        //            description = item.Description,
        //            latitude = item.Latitude,
        //            longitude = item.Longitude,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error fetching item");
        //        return StatusCode(500, new { message = "Error loading item." });
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit(int id, ItemPostDto itemPostDto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(new { message = "Invalid item data" });

        //    try
        //    {
        //        var item = await _itemPostService.GetItemPostByIdAsync(id);
        //        var currentUser = await _userManager.GetUserAsync(User);

        //        if (item == null || currentUser == null || item.UserId != currentUser.Id)
        //            return Forbid();

        //        await _itemPostService.UpdateItemPostAsync(id, itemPostDto);
        //        return Ok(new { message = "Item updated successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error editing item");
        //        return StatusCode(500, new { message = "Error updating item." });
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {
        //        var item = await _itemPostService.GetItemPostByIdAsync(id);
        //        var currentUser = await _userManager.GetUserAsync(User);

        //        if (item == null || currentUser == null || item.UserId != currentUser.Id)
        //            return Forbid();

        //        var result = await _itemPostService.DeleteItemPostAsync(id);
        //        if (!result)
        //            return NotFound();

        //        return Ok(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting item");
        //        return StatusCode(500, new { message = "Error deleting item." });
        //    }
        //}
    }
}