namespace Board.Pages;

using Common.Models;
using Data;
using Infrastructure;
using Microsoft.AspNetCore.Components;

public partial class Profile
{
    [Parameter] public string Id { get; set; }
    public AccountViewModel AccountViewModel { get; set; }
    [Inject] public ImageHelper ImageHelper { get; set; }
    [Inject] private AccountRepository AccountRepository { get; set; }
}
