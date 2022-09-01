using Common.Models;

namespace LoadTesting.DataGenerators;

public class BasicImagesGenerator : IImagesGenerator
{
    private const string RandomImagesSource = "https://picsum.photos/300";

    public async Task<byte[][]> GenerateImages(int count)
    {
        var result = new byte[count][];
        for (int i = 0; i < count; i++)
        {
            var client = new HttpClient();
            result[i] = await client.GetByteArrayAsync(RandomImagesSource);
        }

        return result;
    }
}