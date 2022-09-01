using Microsoft.AspNetCore.Components;

namespace Board.Components
{
    public class LoginRedirectComponent:ComponentBase
    {
        [Inject]
        private NavigationManager Manager { get; set; }
        protected override void OnInitialized()
        {
            Manager.NavigateTo("/login");
        }
    }
}