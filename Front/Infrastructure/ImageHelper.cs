namespace Board.Infrastructure;

using System;

public class ImageHelper
{
    private readonly Uri serviceAddress;

    public ImageHelper(string serviceAddress) =>
        this.serviceAddress = new Uri(new Uri(serviceAddress), Constants.GetImagePostfix);

    public Uri GetImageLink(string id) => new Uri(serviceAddress, id);
}
