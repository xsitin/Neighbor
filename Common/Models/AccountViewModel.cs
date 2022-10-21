namespace Common.Models;

public class AccountViewModel
{
    public string Name { get; init; }
    public string Id { get; init; }
    public string AvatarId { get; init; }


    public AccountViewModel()
    {
    }

    public AccountViewModel(string id, string name, string avatarId)
    {
        Id = id;
        Name = name;
        AvatarId = avatarId;
    }
}
