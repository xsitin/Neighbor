namespace Board.Pages;

using System.Threading.Tasks;
using Common.Models;
using Data;
using Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

public partial class Registration
{
    private IBrowserFile avatar;

    void ImageChanged(InputFileChangeEventArgs obj) => avatar = obj.File;
    [Inject] ILocalStorageService LocalStorage { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }

    [Inject] AccountRepository Repository { get; set; }
    private AccountRegistration AccountData { get; set; } = new AccountRegistration();

    public async Task CreateAccount()
    {
        var token = await Repository.Register(AccountData, avatar);
        if (token is not null)
        {
            await LocalStorage.SetAsync(nameof(SecurityToken), token);
            NavigationManager.NavigateTo("/", true);
        }
    }
}
