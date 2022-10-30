namespace Board.Pages;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Models;
using Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;

public partial class EditProfile
{
    private Account AccountData { get; set; } = new();
    private Account Modified { get; set; } = new();
    private JsonPatchDocument patchDocument = new();
    [Inject] private AccountRepository AccountRepository { get; set; }
    [Inject] private AuthenticationStateProvider provider { get; set; }
    private IBrowserFile avatar { get; set; }

    private async Task SendUpdate()
    {
        AccountRepository.Update(AccountData.Login, patchDocument);
    }

    protected override async Task OnInitializedAsync()
    {
        var userIdentity = (await provider.GetAuthenticationStateAsync()).User.Identity;
        if (userIdentity != null)
        {
            AccountData = await AccountRepository.GetFullAccount(userIdentity.Name);
            AccountData.Password = "password";
            Modified = (Account)AccountData.Clone();
        }
    }

    void ImageChanged(InputFileChangeEventArgs obj) => avatar = obj.File;

    private void ChangeValue(string path, object value)
    {
        Console.WriteLine($"path: {path}; changed on: {value}");
        patchDocument.Replace(path, value);
        patchDocument.ApplyTo(Modified);
    }
}
