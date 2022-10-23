using System;
using Common.Models;
using Microsoft.AspNetCore.Components;

namespace Board.Components;

using Infrastructure;

public partial class PaginationComponent
{
    private string baseAddress = "/";
    [Parameter] public PaginationInfo<Ad> PaginationInfo { get; set; }

    [Parameter]
    public string BaseAddress
    {
        get => baseAddress;
        set
        {
            if (value == baseAddress)
                return;

            baseAddress = value;
            BaseUri = new Uri(BaseUri, value);
        }
    }


    private Uri BaseUri { get; set; } = new Uri(Constants.FrontPath);

    private string IsDisabledPreviousPageArrow => PaginationInfo.Page > 1 ? "" : "disabled";
    private string IsDisabledNextPageArrow => PaginationInfo.Page < PaginationInfo.PageCount ? "" : "disabled";

    public void SetPage(int pageNumber)
    {
        PaginationInfo.Page = pageNumber;
        StateHasChanged();
    }

    private string PageButtonActive(int pageNumber) => PaginationInfo.Page == pageNumber ? "active" : "";

    private string GetPageRelativeAddress(int pageNumber) => new Uri(BaseUri, $"page/{pageNumber}").ToString();
}
