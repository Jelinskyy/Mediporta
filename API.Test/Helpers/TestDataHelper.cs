using API.Dtos;
using API.Entities;

namespace API.Tests;

public class TestDataHelper
{
    public static List<Tag> GetFakeTagList()
    {
        return new List<Tag>()
        {
            new Tag(){
                Name = ".Net",
                HasSynonyms = false,
                IsMadatorOnly = false,
                IsRequired = false,
                Count = 10000
            },
            new Tag(){
                Name = "ASP",
                HasSynonyms = false,
                IsMadatorOnly = false,
                IsRequired = false,
                Count = 5000
            },
            new Tag(){
                Name = "SQLite",
                HasSynonyms = false,
                IsMadatorOnly = false,
                IsRequired = false,
                Count = 5000
            }
        };
    }

    public static List<TagDto> GetFakeTagDtoList()
    {
        return new List<TagDto>()
        {
            new TagDto(){
                Name = ".Net",
                HasSynonyms = false,
                IsMadatorOnly = false,
                IsRequired = false,
                Count = 10000,
                Percent = 50
            },
            new TagDto(){
                Name = "ASP",
                HasSynonyms = false,
                IsMadatorOnly = false,
                IsRequired = false,
                Count = 5000,
                Percent = 25
            },
            new TagDto(){
                Name = "SQLite",
                HasSynonyms = false,
                IsMadatorOnly = false,
                IsRequired = false,
                Count = 5000,
                Percent = 25
            }
        };
    }
}
