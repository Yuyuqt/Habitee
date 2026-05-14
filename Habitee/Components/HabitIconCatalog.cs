using System.Collections.ObjectModel;

namespace Habitee.Components;

public sealed record HabitIconDefinition(string Key, string Label, string Category, string SvgPath);

public static class HabitIconCatalog
{
    public const string FallbackSvgPath = @"<polyline points=""20 6 9 17 4 12""></polyline>";

    public static readonly ReadOnlyCollection<HabitIconDefinition> All = Array.AsReadOnly<HabitIconDefinition>(
    [
        new("water", "Water", "Health & Wellness", @"<path d=""M12 2.69l5.66 5.66a8 8 0 1 1-11.31 0z""></path>"),
        new("pill", "Pill", "Health & Wellness", @"<path d=""M14 2H6a2 2 0 0 0-2 2v16c0 1.1.9 2 2 2h12a2 2 0 0 0 2-2V8l-6-6z""></path><path d=""M14 3v5h5M16 13H8M16 17H8""></path>"),
        new("heart", "Heart", "Health & Wellness", @"<path d=""M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z""></path>"),
        new("meditate", "Meditate", "Health & Wellness", @"<circle cx=""12"" cy=""6"" r=""2""></circle><path d=""M12 10v4""></path><path d=""M8 14h8""></path><path d=""M5 18l3-4""></path><path d=""M19 18l-3-4""></path>"),
        new("tooth", "Tooth", "Health & Wellness", @"<path d=""M12 21a3 3 0 0 1-3-3V6a3 3 0 0 1 6 0v12a3 3 0 0 1-3 3z""></path><path d=""M15 15H9""></path>"),
        new("sleep", "Sleep", "Health & Wellness", @"<path d=""M18 13a6 6 0 1 1-7-7 7 7 0 0 0 7 7z""></path><path d=""M6 5h4""></path><path d=""M4 9h5""></path>"),
        new("stethoscope", "Checkup", "Health & Wellness", @"<path d=""M6 3v5a4 4 0 0 0 8 0V3""></path><path d=""M10 17a4 4 0 1 0 8 0v-1""></path><circle cx=""18"" cy=""14"" r=""2""></circle><path d=""M14 8v5a4 4 0 0 0 4 4""></path>"),
        new("scale", "Scale", "Health & Wellness", @"<path d=""M5 21h14""></path><path d=""M7 21V7h10v14""></path><path d=""M12 11l3-2""></path><circle cx=""12"" cy=""11"" r=""1""></circle>"),
        new("bandage", "Bandage", "Health & Wellness", @"<path d=""M10.5 13.5 13.5 10.5""></path><path d=""M7 7l10 10""></path><path d=""M5.5 8.5 8.5 5.5a3 3 0 0 1 4.24 0l5.76 5.76a3 3 0 0 1 0 4.24l-3 3a3 3 0 0 1-4.24 0L5.5 12.74a3 3 0 0 1 0-4.24z""></path>"),

        new("dumbbell", "Strength", "Fitness & Movement", @"<path d=""M14.4 14.4l5.6-5.6""></path><path d=""M20 12l2 2""></path><path d=""M2 10l2 2""></path><path d=""M9.6 9.6l-5.6 5.6""></path><path d=""M14 2l8 8""></path><path d=""M2 14l8 8""></path>"),
        new("bike", "Bike", "Fitness & Movement", @"<circle cx=""5.5"" cy=""17.5"" r=""3.5""></circle><circle cx=""18.5"" cy=""17.5"" r=""3.5""></circle><path d=""M15 6a1 1 0 1 0 0-2 1 1 0 0 0 0 2zm-3 11.5V14l-3-3 4-3 2 3h2""></path>"),
        new("run", "Run", "Fitness & Movement", @"<path d=""M13 4v4l-4 2""></path><path d=""M13 14l4-4""></path><path d=""M13 18l-3-4""></path><path d=""M16 22l-3-4""></path>"),
        new("walk", "Walk", "Fitness & Movement", @"<circle cx=""13"" cy=""4.5"" r=""1.5""></circle><path d=""M12 7l-2 5 3 2 1 5""></path><path d=""M10 12H7""></path><path d=""M13 10l3 2 2 4""></path>"),
        new("yoga", "Yoga", "Fitness & Movement", @"<circle cx=""12"" cy=""5"" r=""2""></circle><path d=""M12 7v5""></path><path d=""M7 12h10""></path><path d=""M9 12l-3 6""></path><path d=""M15 12l3 6""></path>"),
        new("swim", "Swim", "Fitness & Movement", @"<path d=""M5 18c1.5 1 3 1 4.5 0s3-1 4.5 0 3 1 4.5 0""></path><path d=""M8 10l4-2 4 3""></path><path d=""M12 8V5""></path><circle cx=""12"" cy=""4"" r=""1""></circle>"),
        new("stretch", "Stretch", "Fitness & Movement", @"<circle cx=""12"" cy=""5"" r=""1.5""></circle><path d=""M12 7v5""></path><path d=""M4 12h16""></path><path d=""M9 12l-4 5""></path><path d=""M15 12l4 5""></path>"),
        new("stopwatch", "Timer", "Fitness & Movement", @"<circle cx=""12"" cy=""13"" r=""7""></circle><path d=""M12 13l3-2""></path><path d=""M9 2h6""></path><path d=""M12 6V2""></path>"),
        new("trophy", "Trophy", "Fitness & Movement", @"<path d=""M8 4h8v3a4 4 0 0 1-8 0V4z""></path><path d=""M6 6H4a2 2 0 0 0 2 4h1""></path><path d=""M18 6h2a2 2 0 0 1-2 4h-1""></path><path d=""M12 11v4""></path><path d=""M9 21h6""></path><path d=""M10 17h4""></path>"),

        new("book", "Book", "Learning & Work", @"<path d=""M4 19.5A2.5 2.5 0 0 1 6.5 17H20""></path><path d=""M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z""></path>"),
        new("code", "Code", "Learning & Work", @"<polyline points=""16 18 22 12 16 6""></polyline><polyline points=""8 6 2 12 8 18""></polyline>"),
        new("target", "Target", "Learning & Work", @"<circle cx=""12"" cy=""12"" r=""10""></circle><circle cx=""12"" cy=""12"" r=""6""></circle><circle cx=""12"" cy=""12"" r=""2""></circle>"),
        new("pen", "Write", "Learning & Work", @"<path d=""M12 20h9""></path><path d=""M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4 12.5-12.5z""></path>"),
        new("laptop", "Laptop", "Learning & Work", @"<rect x=""4"" y=""5"" width=""16"" height=""10"" rx=""2""></rect><path d=""M2 19h20""></path>"),
        new("briefcase", "Work", "Learning & Work", @"<rect x=""3"" y=""7"" width=""18"" height=""12"" rx=""2""></rect><path d=""M9 7V5a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2""></path><path d=""M3 12h18""></path>"),
        new("chart", "Progress", "Learning & Work", @"<path d=""M4 19h16""></path><path d=""M7 16V9""></path><path d=""M12 16V5""></path><path d=""M17 16v-7""></path>"),
        new("lightbulb", "Ideas", "Learning & Work", @"<path d=""M9 18h6""></path><path d=""M10 22h4""></path><path d=""M8 10a4 4 0 1 1 8 0c0 1.7-.8 2.7-1.7 3.7-.6.7-1.3 1.4-1.3 2.3h-2c0-.9-.7-1.6-1.3-2.3C8.8 12.7 8 11.7 8 10z""></path>"),
        new("calendar", "Calendar", "Learning & Work", @"<rect x=""3"" y=""4"" width=""18"" height=""17"" rx=""2""></rect><path d=""M16 2v4""></path><path d=""M8 2v4""></path><path d=""M3 10h18""></path>"),

        new("cutlery", "Meals", "Food & Drink", @"<path d=""M3 2v7c0 1.1.9 2 2 2h4a2 2 0 0 0 2-2V2""></path><path d=""M7 2v20""></path><path d=""M21 15V2v0a5 5 0 0 0-5 5v6c0 1.1.9 2 2 2h3zm0 0v7""></path>"),
        new("burger", "Burger", "Food & Drink", @"<path d=""M3 11h18""></path><path d=""M4 11a8 8 0 0 1 16 0""></path><path d=""M4 17a4 4 0 0 0 4 4h8a4 4 0 0 0 4-4""></path><path d=""M3 14h18""></path>"),
        new("soda", "Soda", "Food & Drink", @"<path d=""M6 5h12""></path><path d=""M8 5v14a2 2 0 0 0 2 2h4a2 2 0 0 0 2-2V5""></path><path d=""M9 2l3 3 3-3""></path>"),
        new("sugar", "Sugar", "Food & Drink", @"<path d=""M12 2v20""></path><path d=""M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6""></path>"),
        new("apple", "Apple", "Food & Drink", @"<path d=""M12 7c-2.5-2.5-6-.8-6 3.5 0 4.7 2.6 9.5 6 9.5s6-4.8 6-9.5c0-4.3-3.5-6-6-3.5z""></path><path d=""M12 7c0-2 1.4-3.5 3.5-4""></path><path d=""M10 3c1 .8 1.5 1.7 2 3""></path>"),
        new("carrot", "Carrot", "Food & Drink", @"<path d=""M9 14c3-5 7-8 10-9-1 4-4 8-9 10l-3 5-3-3 5-3z""></path><path d=""M15 4l2-2""></path><path d=""M17 6l3-1""></path>"),
        new("coffee", "Coffee", "Food & Drink", @"<path d=""M5 8h11v5a4 4 0 0 1-4 4H9a4 4 0 0 1-4-4V8z""></path><path d=""M16 10h2a2 2 0 0 1 0 4h-2""></path><path d=""M8 3c0 1 .5 1.5.5 2.5S8 7 8 8""></path><path d=""M12 3c0 1 .5 1.5.5 2.5S12 7 12 8""></path>"),
        new("tea", "Tea", "Food & Drink", @"<path d=""M6 9h10v4a4 4 0 0 1-4 4h-2a4 4 0 0 1-4-4V9z""></path><path d=""M16 10h2a2 2 0 0 1 0 4h-2""></path><path d=""M9 5h6""></path>"),
        new("bowl", "Bowl", "Food & Drink", @"<path d=""M4 10h16a8 8 0 0 1-16 0z""></path><path d=""M8 6c0-1.5 1-2.5 2-3""></path><path d=""M12 6c0-1.5 1-2.5 2-3""></path>"),

        new("home", "Home", "Home & Routine", @"<path d=""M3 10.5 12 3l9 7.5""></path><path d=""M5 9.5V21h14V9.5""></path><path d=""M10 21v-6h4v6""></path>"),
        new("bed", "Bed", "Home & Routine", @"<path d=""M3 12h18v6H3z""></path><path d=""M3 18V8""></path><path d=""M7 12V9h4a3 3 0 0 1 3 3""></path>"),
        new("shower", "Shower", "Home & Routine", @"<path d=""M6 8a4 4 0 0 1 8 0v1""></path><path d=""M14 9h4""></path><path d=""M18 9v4""></path><path d=""M9 14v1""></path><path d=""M12 14v2""></path><path d=""M15 14v1""></path>"),
        new("laundry", "Laundry", "Home & Routine", @"<rect x=""4"" y=""3"" width=""16"" height=""18"" rx=""2""></rect><circle cx=""12"" cy=""13"" r=""4""></circle><path d=""M8 7h.01""></path><path d=""M11 7h.01""></path>"),
        new("broom", "Clean", "Home & Routine", @"<path d=""M14 3l7 7""></path><path d=""M4 20l10-10""></path><path d=""M3 21l4-1 2-2-3-3-2 2-1 4z""></path>"),
        new("plant", "Plant", "Home & Routine", @"<path d=""M12 21v-7""></path><path d=""M8 21h8""></path><path d=""M12 14c-4 0-6-3-6-7 4 0 6 3 6 7z""></path><path d=""M12 14c4 0 6-3 6-7-4 0-6 3-6 7z""></path>"),
        new("key", "Keys", "Home & Routine", @"<circle cx=""8"" cy=""15"" r=""3""></circle><path d=""M11 15h10""></path><path d=""M18 15v-2""></path><path d=""M15 15v2""></path>"),
        new("shopping", "Shopping", "Home & Routine", @"<path d=""M6 7h15l-1.5 8h-11z""></path><path d=""M6 7 5 4H3""></path><circle cx=""10"" cy=""19"" r=""1""></circle><circle cx=""18"" cy=""19"" r=""1""></circle>"),
        new("recycle", "Recycle", "Home & Routine", @"<path d=""M10 3 8 7h4z""></path><path d=""M14 7h3l-2 4""></path><path d=""M7 13l-2 4h4""></path><path d=""M13 17h-3l2-4""></path><path d=""M18 11l2 4h-4""></path>"),

        new("journal", "Journal", "Mindfulness & Creativity", @"<rect x=""5"" y=""3"" width=""14"" height=""18"" rx=""2""></rect><path d=""M9 3v18""></path><path d=""M12 8h4""></path><path d=""M12 12h4""></path>"),
        new("music", "Music", "Mindfulness & Creativity", @"<path d=""M9 18V6l10-2v12""></path><circle cx=""7"" cy=""18"" r=""2""></circle><circle cx=""17"" cy=""16"" r=""2""></circle>"),
        new("paint", "Paint", "Mindfulness & Creativity", @"<path d=""M12 3a9 9 0 1 0 0 18h1a2 2 0 0 0 0-4h-1a2 2 0 0 1 0-4h1a3 3 0 0 0 0-6h-1z""></path><circle cx=""8"" cy=""10"" r=""1""></circle><circle cx=""12"" cy=""7"" r=""1""></circle><circle cx=""16"" cy=""10"" r=""1""></circle>"),
        new("camera", "Camera", "Mindfulness & Creativity", @"<rect x=""3"" y=""7"" width=""18"" height=""12"" rx=""2""></rect><path d=""M8 7l2-3h4l2 3""></path><circle cx=""12"" cy=""13"" r=""3""></circle>"),
        new("sparkles", "Sparkles", "Mindfulness & Creativity", @"<path d=""M12 3l1.5 4.5L18 9l-4.5 1.5L12 15l-1.5-4.5L6 9l4.5-1.5z""></path><path d=""M19 3v4""></path><path d=""M21 5h-4""></path><path d=""M4 16v3""></path><path d=""M5.5 17.5h-3""></path>"),
        new("puzzle", "Puzzle", "Mindfulness & Creativity", @"<path d=""M8 3h4a2 2 0 1 1 4 0h2a2 2 0 0 1 2 2v4h-3a2 2 0 1 0 0 4h3v4a2 2 0 0 1-2 2h-4v-3a2 2 0 1 0-4 0v3H6a2 2 0 0 1-2-2v-4h3a2 2 0 1 0 0-4H4V5a2 2 0 0 1 2-2h2z""></path>"),
        new("candle", "Candle", "Mindfulness & Creativity", @"<path d=""M9 9h6v10H9z""></path><path d=""M12 3c1.5 1 2 2.5 2 4a2 2 0 1 1-4 0c0-1.5.5-3 2-4z""></path><path d=""M8 21h8""></path>"),
        new("headphones", "Headphones", "Mindfulness & Creativity", @"<path d=""M5 13a7 7 0 0 1 14 0""></path><rect x=""4"" y=""13"" width=""3"" height=""6"" rx=""1""></rect><rect x=""17"" y=""13"" width=""3"" height=""6"" rx=""1""></rect>"),
        new("flower", "Flower", "Mindfulness & Creativity", @"<circle cx=""12"" cy=""12"" r=""2""></circle><circle cx=""12"" cy=""7"" r=""2""></circle><circle cx=""17"" cy=""12"" r=""2""></circle><circle cx=""12"" cy=""17"" r=""2""></circle><circle cx=""7"" cy=""12"" r=""2""></circle>"),

        new("tree", "Tree", "Outdoors & Travel", @"<path d=""M12 3 6 11h4l-3 5h4l-2 5""></path><path d=""M12 3 18 11h-4l3 5h-4l2 5""></path><path d=""M12 16v5""></path>"),
        new("mountain", "Mountain", "Outdoors & Travel", @"<path d=""M3 19 10 8l4 6 3-4 4 9H3z""></path>"),
        new("sun", "Sun", "Outdoors & Travel", @"<circle cx=""12"" cy=""12"" r=""4""></circle><path d=""M12 2v3""></path><path d=""M12 19v3""></path><path d=""M2 12h3""></path><path d=""M19 12h3""></path><path d=""m4.9 4.9 2.1 2.1""></path><path d=""m17 17 2.1 2.1""></path><path d=""m19.1 4.9-2.1 2.1""></path><path d=""m7 17-2.1 2.1""></path>"),
        new("tent", "Camping", "Outdoors & Travel", @"<path d=""M3 19 12 5l9 14""></path><path d=""M9 19l3-5 3 5""></path>"),
        new("compass", "Compass", "Outdoors & Travel", @"<circle cx=""12"" cy=""12"" r=""9""></circle><path d=""M15 9l-2 6-4 1 2-6 4-1z""></path>"),
        new("map", "Map", "Outdoors & Travel", @"<path d=""M3 6l6-2 6 2 6-2v14l-6 2-6-2-6 2V6z""></path><path d=""M9 4v14""></path><path d=""M15 6v14""></path>"),
        new("car", "Drive", "Outdoors & Travel", @"<path d=""M5 15l1.5-5h11L19 15""></path><path d=""M4 15h16v4H4z""></path><circle cx=""7"" cy=""19"" r=""1""></circle><circle cx=""17"" cy=""19"" r=""1""></circle>"),
        new("plane", "Travel", "Outdoors & Travel", @"<path d=""M2 16l20-4-20-4 5 4-5 4z""></path><path d=""M7 12h8""></path><path d=""M12 8l2-4""></path><path d=""M12 16l2 4""></path>"),
        new("leaf", "Nature", "Outdoors & Travel", @"<path d=""M19 3c-7 0-12 5-12 12 0 3 2 6 5 6 7 0 12-5 12-12 0-3-2-6-5-6z""></path><path d=""M8 16c2-2 5-4 9-5""></path>"),

        new("phone", "Phone", "Social & Lifestyle", @"<rect x=""7"" y=""2"" width=""10"" height=""20"" rx=""2""></rect><path d=""M11 18h2""></path>"),
        new("chat", "Chat", "Social & Lifestyle", @"<path d=""M4 5h16v10H8l-4 4V5z""></path>"),
        new("users", "Friends", "Social & Lifestyle", @"<circle cx=""9"" cy=""8"" r=""3""></circle><circle cx=""17"" cy=""9"" r=""2.5""></circle><path d=""M4 19a5 5 0 0 1 10 0""></path><path d=""M14 19a4 4 0 0 1 6 0""></path>"),
        new("gift", "Gift", "Social & Lifestyle", @"<rect x=""3"" y=""8"" width=""18"" height=""13"" rx=""2""></rect><path d=""M12 8v13""></path><path d=""M3 12h18""></path><path d=""M12 8H8a2 2 0 1 1 0-4c2 0 4 4 4 4z""></path><path d=""M12 8h4a2 2 0 1 0 0-4c-2 0-4 4-4 4z""></path>"),
        new("pet", "Pet", "Social & Lifestyle", @"<circle cx=""8"" cy=""8"" r=""1.5""></circle><circle cx=""16"" cy=""8"" r=""1.5""></circle><circle cx=""6"" cy=""12"" r=""1.5""></circle><circle cx=""18"" cy=""12"" r=""1.5""></circle><path d=""M8 18c1.5 2 6.5 2 8 0 .8-1 1-3 0-4s-2.5-1-4-.2c-1.5-.8-3-.8-4 .2s-.8 3 0 4z""></path>"),
        new("gamepad", "Game", "Social & Lifestyle", @"<rect x=""4"" y=""9"" width=""16"" height=""8"" rx=""4""></rect><path d=""M8 13h4""></path><path d=""M10 11v4""></path><path d=""M16 12h.01""></path><path d=""M18 14h.01""></path>"),
        new("film", "Movie", "Social & Lifestyle", @"<rect x=""3"" y=""5"" width=""18"" height=""14"" rx=""2""></rect><path d=""M7 5v14""></path><path d=""M17 5v14""></path><path d=""M3 9h4""></path><path d=""M17 9h4""></path><path d=""M3 15h4""></path><path d=""M17 15h4""></path>"),
        new("smile", "Mood", "Social & Lifestyle", @"<circle cx=""12"" cy=""12"" r=""9""></circle><path d=""M8 10h.01""></path><path d=""M16 10h.01""></path><path d=""M8 15a5 5 0 0 0 8 0""></path>"),
        new("handshake", "Connect", "Social & Lifestyle", @"<path d=""M7 13l3 3a2 2 0 0 0 2.8 0l4.2-4.2a2 2 0 0 1 2.8 0L22 14""></path><path d=""M2 10l3-3a2 2 0 0 1 2.8 0L11 10""></path><path d=""M2 20l5-5""></path><path d=""M17 7l5 5""></path>")
    ]);

    private static readonly Dictionary<string, HabitIconDefinition> Lookup = All
        .ToDictionary(icon => icon.Key, StringComparer.OrdinalIgnoreCase);

    public static HabitIconDefinition? Find(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return Lookup.TryGetValue(key, out var icon) ? icon : null;
    }

    public static string GetSvgPath(string? key)
    {
        return Find(key)?.SvgPath ?? FallbackSvgPath;
    }
}
