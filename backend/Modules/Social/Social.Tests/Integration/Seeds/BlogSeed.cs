using Shared.Tests;
using Social.Domain.Blogs;

namespace Social.Tests.Integration.Seeds;

internal static class BlogSeed
{
    public static readonly Blog FortressImpressions = new(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Obilazak Petrovaradinske tvrđave ostavio je snažan utisak: podzemne galerije, Sahat kula i pogled na Dunav.", ["https://example.com/tvrdjava.jpg"]);
    public static readonly Blog CityNotes = new(WellKnownUsers.Explorer, "Beleške iz grada", "Kratke beleške sa šetnje centrom grada.", []);
    public static readonly Blog SecondExplorersDiary = new(WellKnownUsers.SecondExplorer, "Tuđi dnevnik", "Beleške drugog autora, vidljive samo njemu.", []);
    public static readonly Blog PublishedRiverbank;
    public static readonly Blog CommentedMuseumVisit;

    static BlogSeed()
    {
        PublishedRiverbank = new(WellKnownUsers.Explorer, "Zalasci na keju", "Fotografije i utisci sa večernje šetnje novosadskim kejom.", []);
        PublishedRiverbank.Publish();

        CommentedMuseumVisit = new(WellKnownUsers.Explorer, "Poseta muzeju", "Utisci sa stalne postavke Muzeja Vojvodine.", []);
        CommentedMuseumVisit.Publish();
        CommentedMuseumVisit.AddComment(WellKnownUsers.Administrator, "Odlična preporuka!");
    }

    public static Blog[] All => [FortressImpressions, CityNotes, SecondExplorersDiary, PublishedRiverbank, CommentedMuseumVisit];
}
