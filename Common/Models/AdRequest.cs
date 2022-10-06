namespace Common.Models;

using System;

public class AdRequest
{
    public DataRequestTypes Types { get; init; }
    public string Username { get; init; }
    public string Query { get; init; }
    public string Category { get; init; }
    public PaginationInfo<Ad> PaginationInfo { get; init; }

    public AdRequest(DataRequestTypes types, string username = "", string query = "", string category = "",
        PaginationInfo<Ad> paginationInfo = null)
    {
        Types = types;
        Username = username;
        Query = query;
        Category = category;
        PaginationInfo = paginationInfo;
    }
}

[Flags]
public enum DataRequestTypes
{
    None = 0,
    Popular = 1,
    Search = 2,
    FromUser = 4,
    FromCategory = 8
}
