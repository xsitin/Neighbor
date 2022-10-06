using System;
using System.Text.Json;
using System.Threading.Tasks;
using Board.Data;
using Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Board.Pages;

public partial class Index
{
    [Parameter]
    public int Page
    {
        get => PageInfo.Page;
        set => PageInfo.Page = value >= 1 ? value : 1;
    }

    private string SearchTitle { get; set; }
    [Inject] private AdsRepository Repository { get; set; }

    [Inject] public NavigationManager NavigationManager { get; set; }

    private PaginationInfo<Ad> PageInfo { get; set; } = new PaginationInfo<Ad>() {PageSize = 21, Page = 1};


    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine(JsonSerializer.Serialize(PageInfo));
        if (PageInfo != null)
            PageInfo = await Repository.GetPopularAsync(PageInfo.Page, PageInfo.PageSize);
        else
            PageInfo = await Repository.GetPopularAsync();


        async void AdsUpdater(object sender, LocationChangedEventArgs args)
        {
            if (args.Location == NavigationManager.Uri)
            {
                if (PageInfo != null)
                    PageInfo = await Repository.GetPopularAsync(PageInfo.Page, PageInfo.PageSize);
                else
                    PageInfo = await Repository.GetPopularAsync();
                StateHasChanged();
            }
            else
                NavigationManager.LocationChanged -= AdsUpdater;
        }

        NavigationManager.LocationChanged += AdsUpdater;
    }

    public async Task CategorySelected(string category)
    {
        PageInfo = await Repository.GetWithCategory(category);
    }


    public async Task SearchAdsWithSelectedTitle()
    {
        if (string.IsNullOrEmpty(SearchTitle?.Trim()))
        {
            PageInfo = await Repository.GetPopularAsync();
        }
        else
        {
            PageInfo = await Repository.SearchWithTitle(SearchTitle);
        }
    }
}
