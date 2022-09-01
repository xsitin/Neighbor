using System;
using Common.Models;
using Microsoft.AspNetCore.Components;

namespace Board.Components;

public partial class PaginationComponent
{
    [Parameter] public PaginationInfo<Ad> PaginationInfo { get; set; }
    [Parameter] public Action CallUpdate { get; set; }

    private string IsDisabledPreviousPageArrow => PaginationInfo.Page > 1 ? "" : "disabled";
    private string IsDisabledNextPageArrow => PaginationInfo.Page < PaginationInfo.PageCount ? "" : "disabled";

    public void SetPage(int page)
    {
        PaginationInfo.Page = page;
        StateHasChanged();
    }
}