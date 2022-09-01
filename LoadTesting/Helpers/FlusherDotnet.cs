using MongoDB.Driver;

namespace LoadTesting.Helpers;

public class FlusherDotnet : IFlusher
{
    private static IMongoClient Client { get; set; } = null!;
    private static readonly string adCollectionName = "Ads";
    private static readonly string accountCollectionName = "Accounts";
    private static readonly string imageCollectionName = "Images";
    private static IMongoDatabase database { get; set; }
    private static readonly string dbName = "Board";
    private const string ImagesPath = @"C:\Users\xsitin\RiderProjects\Neigbor\WebApi\boardImages";

    public FlusherDotnet()
    {
        Client ??= new MongoClient();
        database ??= Client.GetDatabase(dbName);
    }


    public void FlushAll()
    {
        FlushAccounts();
        FlushAd();
    }

    public void FlushAccounts()
    {
        database.DropCollection(accountCollectionName);
    }

    public void FlushAd()
    {
        database.DropCollection(adCollectionName);
        database.DropCollection(imageCollectionName);
        Directory.Delete(ImagesPath,true);
    }
}