using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Board.Infrastructure;
using Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Board.Pages;

public partial class Index
{
    public List<Ad> Ads { get; set; } = new();

    [Parameter]
    public int Page
    {
        get => PaginationInfo.Page;
        set => PaginationInfo.Page = value >= 1 ? value : 1;
    }
    
    private string SearchTitle { get; set; }
    [Inject] private AdsRepository Repository { get; set; }

    [Inject] public NavigationManager NavigationManager { get; set; }

    private PaginationInfo<Ad> PaginationInfo { get; set; } = new PaginationInfo<Ad>() {PageSize = 21, Page = 1};

    public void Update()
    {
        PaginationInfo = Repository.GetPopularAsync(PaginationInfo.Page, PaginationInfo.PageSize).Result;
    }

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine(JsonSerializer.Serialize(PaginationInfo));
        if (PaginationInfo != null)
            PaginationInfo = await Repository.GetPopularAsync(PaginationInfo.Page, PaginationInfo.PageSize);
        else
            PaginationInfo = await Repository.GetPopularAsync();

        Ads.AddRange(PaginationInfo.Items);

        async void AdsUpdater(object sender, LocationChangedEventArgs args)
        {
            if (args.Location == NavigationManager.Uri)
            {
                if (PaginationInfo != null)
                    PaginationInfo = await Repository.GetPopularAsync(PaginationInfo.Page, PaginationInfo.PageSize);
                else
                    PaginationInfo = await Repository.GetPopularAsync();
                Ads = PaginationInfo.Items;
                StateHasChanged();
            }
            else
                NavigationManager.LocationChanged -= AdsUpdater;
        }

        NavigationManager.LocationChanged += AdsUpdater;
    }

    public async Task CategorySelected(string category)
    {
        Ads = (await Repository.GetWithCategory(category)).ToList();
    }

    public async Task SearchAdsWithSelectedTitle()
    {
        if (string.IsNullOrEmpty(SearchTitle?.Trim()))
        {
            Ads = new(Repository.GetPopularAsync().Result.Items);
        }
        else
            Ads = (await Repository.SearchWithTitle(SearchTitle)).ToList();
    }
}