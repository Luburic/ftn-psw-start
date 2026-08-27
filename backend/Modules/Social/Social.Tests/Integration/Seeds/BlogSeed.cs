using Shared.Tests;
using Social.Domain.Blogs;

namespace Social.Tests.Integration.Seeds;

internal static class BlogSeed
{
    public static readonly Blog FortressImpressions = new(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Obilazak Petrovaradinske tvrđave ostavio je snažan utisak: podzemne galerije, Sahat kula i pogled na Dunav.", ["https://example.com/tvrdjava.jpg"]);
    public static readonly Blog CityNotes = new(WellKnownUsers.Explorer, "Beleške iz grada", "Kratke beleške sa šetnje centrom grada.", []);

    public static object[] All => [FortressImpressions, CityNotes];
}
