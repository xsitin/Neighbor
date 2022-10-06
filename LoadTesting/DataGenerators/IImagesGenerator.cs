namespace LoadTesting.DataGenerators;

public interface IImagesGenerator
{
    Task<byte[][]> GenerateImages(int count);
}