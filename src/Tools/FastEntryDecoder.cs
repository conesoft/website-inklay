﻿using System.Net;
using System.Text.RegularExpressions;
using static Conesoft.Website.Inklay.Components.Controls.Content;

namespace Conesoft_Website_Kontrol.Tools;

public partial class FastEntryDecoder
{
    public static string DecodeDescription(Entry entry)
    {
        var reg = MyRegex().Replace(entry?.Description ?? "", string.Empty);
        return WebUtility.HtmlDecode(reg);
    }

    [GeneratedRegex("<.*?>")]
    private static partial Regex MyRegex();
}