namespace Board.Components;

using System;
using Microsoft.AspNetCore.Components;

public partial class EditableField
{
    private string value;

    [Parameter]
    public string Value
    {
        get => value;
        set
        {
            if (this.value != null && this.value != value) OnChange?.Invoke(value);

            this.value = value;
        }
    }


    [Parameter] public string Label { get; set; } = "";
    [Parameter] public Action<string> OnChange { get; set; }
    [Parameter] public bool IsDisabled { get; set; } = true;

    private void OnClick()
    {
        IsDisabled = !IsDisabled;
    }
}
