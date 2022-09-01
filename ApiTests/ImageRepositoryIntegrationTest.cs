using System;
using System.IO;
using WebApi.Infrastructure;
using Common.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NUnit.Framework;

namespace ApiTests;

public class ImageRepositoryIntegrationTest
{
    private ImageRepository _repository;
    private const string pathToFolderWithFiles = "./images";

    [SetUp]
    public void SetUp()
    {
        Directory.CreateDirectory(pathToFolderWithFiles);
        var mongoCollection = A.Dummy<IMongoCollection<ImageDocument>>();

        _repository = new ImageRepository(mongoCollection, pathToFolderWithFiles);
    }

    [Test]
    public void Add()
    {
        _repository.Add(new FormFile(Stream.Null, 0, 0, "some name", "fileName"));
        Assert.IsNotEmpty(Directory.GetFiles(pathToFolderWithFiles, "*", SearchOption.AllDirectories));
    }

    [Test]
    public void Delete()
    {
        var imageDoc = _repository.Add(new FormFile( new MemoryStream(new byte[1024000]), 0, 1024000, "some name", "fileName")).Result;
        _repository.DeleteAllAsync(new[] {imageDoc.Id});
        Assert.IsEmpty(Directory.GetFiles(pathToFolderWithFiles, "*", SearchOption.AllDirectories));
    }

    [TearDown]
    public void AfterEach()
    {
        Directory.Delete(pathToFolderWithFiles, true);
    }
}