using System.Linq.Expressions;
using API.Controllers;
using API.Data;
using API.Dtos;
using API.Entities;
using API.Helpers;
using API.Tests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.EntityFrameworkCore;


namespace API.Test;

public class TagControllerTests
{
    public readonly DataContext _dataContext;

    public TagControllerTests()
    {
        var options = new DbContextOptionsBuilder()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
        _dataContext = new (options);
    }

    [Fact]
    public async Task GetTags_SortingAcsByName_WithoutCashedValue()
    {
        var memoryCache = Mock.Of<IMemoryCache>();
        var memoryCasheMock = Mock.Get(memoryCache);
        //Arrange
        await _dataContext.AddRangeAsync(TestDataHelper.GetFakeTagList());
        await _dataContext.SaveChangesAsync();

        object? expected;
        memoryCasheMock.Setup(x => x.TryGetValue(It.IsAny<string>(), out expected))
            .Returns(false);

        var casheEntry = Mock.Of<ICacheEntry>();
        memoryCasheMock.Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(casheEntry);

        //Act
        TagController tagController = new(_dataContext, memoryCasheMock.Object);
        var result = (await tagController.GetTags(new GetTagsParams())).Result as OkObjectResult;
        
        //Assert
        Assert.NotNull(result);

        var tags = result.Value as IEnumerable<TagDto>;
        Assert.NotNull(tags);
        Assert.Equal(3, tags.Count());

        Assert.Equal("ASP", tags.First().Name);
        Assert.Equal(".Net", tags.Last().Name);

        Assert.Equal(25, tags.First().Percent);
        Assert.Equal(50, tags.Last().Percent);

        memoryCasheMock.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task GetTags_SortingDescByPercent_WithCashedValue()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();

        var memoryCache = serviceProvider.GetService<IMemoryCache>();

        var getParams = new GetTagsParams(){
            order = OrderEnum.desc,
            sort = SortEnum.percent
        };

        string casheKey = "NotAllTags";
        var casheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(45))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));
                    
        memoryCache.Set(casheKey, TestDataHelper.GetFakeTagDtoList(), casheEntryOptions);

        //Act
        TagController tagController = new(_dataContext, memoryCache, casheKey);
        var result = (await tagController.GetTags(getParams)).Result as OkObjectResult;
        
        //Assert
        Assert.NotNull(result);

        var tags = result.Value as IEnumerable<TagDto>;
        Assert.NotNull(tags);
        Assert.Equal(3, tags.Count());

        Assert.Equal(".Net", tags.First().Name);

        Assert.Equal(50, tags.First().Percent);
        Assert.Equal(25, tags.Last().Percent);

        Assert.Empty(await _dataContext.Tags.ToListAsync());
    }

    // [Fact]
    // public void GetTags_CallingFetchWhenDbIsEmpty()
    // {

    // }

    // [Fact]
    // public void FetchTags_IsSendingRequest()
    // {

    // }
}