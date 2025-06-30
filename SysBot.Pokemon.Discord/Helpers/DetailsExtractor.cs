using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SysBot.Pokemon.Discord;

public class DetailsExtractor<T> where T : PKM, new()
{
    public static EmbedData ExtractPokemonDetails(T pk, SocketUser user, bool isMysteryEgg, bool isCloneRequest, bool isDumpRequest, bool isFixOTRequest, bool isSpecialRequest, bool isBatchTrade, int batchTradeNumber, int totalBatchTrades)
    {
        var strings = GameInfo.GetStrings(pk.Language);
        var embedData = new EmbedData
        {
            // Basic Pokémon details
            Moves = GetMoveNames(pk),
            Level = pk.CurrentLevel
        };

        // Pokémon appearance and type details
        if (pk is PK9 pk9)
        {
            embedData.TeraType = GetTeraTypeString(pk9);
            embedData.Scale = GetScaleDetails(pk9);
        }

        // Pokémon identity and special attributes
        embedData.Ability = GetAbilityName(pk);
        embedData.Nature = GetNatureName(pk);
        embedData.SpeciesName = strings.Species[pk.Species];
        embedData.SpecialSymbols = GetSpecialSymbols(pk);
        embedData.FormName = ShowdownParsing.GetStringFromForm(pk.Form, strings, pk.Species, pk.Context);
        embedData.HeldItem = strings.itemlist[pk.HeldItem];
        embedData.Ball = strings.balllist[pk.Ball];

        // Display elements
        Span<int> ivs = stackalloc int[6];
        pk.GetIVs(ivs);
        string ivsDisplay;
        if (ivs.ToArray().All(iv => iv == 31))
        {
            ivsDisplay = "6IV";
        }
        else
        {

            ivsDisplay = string.Join("/", new[]
             {
                ivs[0].ToString(),
                ivs[1].ToString(),
                ivs[2].ToString(),
                ivs[4].ToString(),
                ivs[5].ToString(),
                ivs[3].ToString()
          });
        }
        embedData.IVsDisplay = ivsDisplay;

        int[] evs = GetEVs(pk);
        string evsDisplay = string.Join(" / ", new[] {
            (evs[0] != 0 ? $"{evs[0]} HP" : ""),
            (evs[1] != 0 ? $"{evs[1]} Atk" : ""),
            (evs[2] != 0 ? $"{evs[2]} Def" : ""),
            (evs[4] != 0 ? $"{evs[4]} SpA" : ""),
            (evs[5] != 0 ? $"{evs[5]} SpD" : ""),
            (evs[3] != 0 ? $"{evs[3]} Spe" : "")
        }.Where(s => !string.IsNullOrEmpty(s)));
        embedData.EVsDisplay = evsDisplay;
        embedData.MetDate = pk.MetDate.ToString();
        embedData.MetLevel = pk.MetLevel;
        embedData.MovesDisplay = string.Join("\n", embedData.Moves);
        embedData.PokemonDisplayName = pk.IsNicknamed ? pk.Nickname : embedData.SpeciesName;
        embedData.Language = pk.Language;

        // Trade title
        embedData.TradeTitle = GetTradeTitle(isMysteryEgg, isCloneRequest, isDumpRequest, isFixOTRequest, isSpecialRequest, isBatchTrade, batchTradeNumber, embedData.PokemonDisplayName, pk.IsShiny);

        // Author name
        embedData.AuthorName = GetAuthorName(user.Username, embedData.TradeTitle, isMysteryEgg, isFixOTRequest, isCloneRequest, isDumpRequest, isSpecialRequest, isBatchTrade, embedData.PokemonDisplayName, pk.IsShiny);

        return embedData;
    }

    private static List<string> GetMoveNames(T pk)
    {
        ushort[] moves = new ushort[4];
        pk.GetMoves(moves.AsSpan());
        List<int> movePPs = [pk.Move1_PP, pk.Move2_PP, pk.Move3_PP, pk.Move4_PP];
        var moveNames = new List<string>();

        var typeEmojis = SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.CustomTypeEmojis
            .Where(e => !string.IsNullOrEmpty(e.EmojiCode))
            .ToDictionary(e => (PKHeX.Core.MoveType)e.MoveType, e => $"{e.EmojiCode}");

        for (int i = 0; i < moves.Length; i++)
        {
            if (moves[i] == 0) continue;
            string moveName = GameInfo.MoveDataSource.FirstOrDefault(m => m.Value == moves[i])?.Text ?? "";
            byte moveTypeId = MoveInfo.GetType(moves[i], default);
            PKHeX.Core.MoveType moveType = (PKHeX.Core.MoveType)moveTypeId;
            string formattedMove = $"*{moveName}* ({movePPs[i]} PP)";
            if (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.MoveTypeEmojis && typeEmojis.TryGetValue(moveType, out var moveEmoji))
            {
                formattedMove = $"{moveEmoji} {formattedMove}";
            }
            moveNames.Add($"\u200B{formattedMove}");
        }

        return moveNames;
    }

    private static string GetTeraTypeString(PK9 pk9)
    {
        var isStellar = pk9.TeraTypeOverride == (MoveType)TeraTypeUtil.Stellar || (int)pk9.TeraType == 99;
        var teraType = isStellar ? TradeSettings.MoveType.Stellar : (TradeSettings.MoveType)pk9.TeraType;

        if (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.UseTeraEmojis)
        { 
            var emojiInfo = SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.TeraTypeEmojis.Find(e => e.MoveType == teraType);
            if (emojiInfo != null && !string.IsNullOrEmpty(emojiInfo.EmojiCode))
            {
                return emojiInfo.EmojiCode;
            }
        }

        return teraType.ToString();
    }

    private static string GetLanguageText(int languageValue)
    {
        switch (languageValue)
        {
            case 1:
                return "Japanese";
            case 2:
                return "English";
            case 3:
                return "French";
            case 4:
                return "Italian";
            case 5:
                return "German";
            case 7:
                return "Spanish";
            case 8:
                return "Korean";
            case 9:
                return "ChineseS";
            case 10:
                return "ChineseT";
            default:
                return "Unknown";
        }
    }

    private static (string, byte) GetScaleDetails(PK9 pk9)
    {
        string scaleText = $"{PokeSizeDetailedUtil.GetSizeRating(pk9.Scale)}";
        byte scaleNumber = pk9.Scale;
        return (scaleText, scaleNumber);
    }

    private static string GetAbilityName(T pk)
    {
        return GameInfo.AbilityDataSource.FirstOrDefault(a => a.Value == pk.Ability)?.Text ?? "";
    }

    private static string GetNatureName(T pk)
    {
        return GameInfo.NatureDataSource.FirstOrDefault(n => n.Value == (int)pk.Nature)?.Text ?? "";
    }

    private static string GetSpecialSymbols(T pk)
    {
        string alphaMarkSymbol = string.Empty;
        string mightyMarkSymbol = string.Empty;
        string markTitle = string.Empty;
        if (pk is IRibbonSetMark9 ribbonSetMark)
        {
            alphaMarkSymbol = ribbonSetMark.RibbonMarkAlpha ? SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.AlphaMarkEmoji.EmojiString : string.Empty;
            mightyMarkSymbol = ribbonSetMark.RibbonMarkMightiest ? SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.MightiestMarkEmoji.EmojiString : string.Empty;
        }
        if (pk is IRibbonIndex ribbonIndex)
        {
            AbstractTrade<T>.HasMark(ribbonIndex, out RibbonIndex result, out markTitle);
        }
        string alphaSymbol = (pk is IAlpha alpha && alpha.IsAlpha) ? SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.AlphaPLAEmoji.EmojiString : string.Empty;
        string shinySymbol = pk.ShinyXor == 0 ? "◼ " : pk.IsShiny ? "★ " : string.Empty;
        string genderSymbol = GameInfo.GenderSymbolASCII[pk.Gender];
        string maleEmojiString = SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.MaleEmoji.EmojiString;
        string femaleEmojiString = SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.FemaleEmoji.EmojiString;
        string displayGender = genderSymbol switch
        {
            "M" => !string.IsNullOrEmpty(maleEmojiString) ? maleEmojiString : "(M) ",
            "F" => !string.IsNullOrEmpty(femaleEmojiString) ? femaleEmojiString : "(F) ",
            _ => ""
        };
        string mysteryGiftEmoji = pk.FatefulEncounter ? SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.MysteryGiftEmoji.EmojiString : "";

        return shinySymbol + alphaSymbol + mightyMarkSymbol + alphaMarkSymbol + mysteryGiftEmoji + displayGender + (!string.IsNullOrEmpty(markTitle) ? $"{markTitle} " : "");
    }

    private static string GetTradeTitle(bool isMysteryEgg, bool isCloneRequest, bool isDumpRequest, bool isFixOTRequest, bool isSpecialRequest, bool isBatchTrade, int batchTradeNumber, string pokemonDisplayName, bool isShiny)
    {
        string shinyEmoji = isShiny ? "✨ " : "";
        return isMysteryEgg ? "✨ Shiny Mystery Egg ✨" :
               isBatchTrade ? $"Batch Trade #{batchTradeNumber} - {shinyEmoji}{pokemonDisplayName}" :
               isFixOTRequest ? "FixOT Request" :
               isSpecialRequest ? "Special Request" :
               isCloneRequest ? "Clone Request!" :
               isDumpRequest ? "Pokémon Dump" :
               "";
    }

    private static string GetAuthorName(string username, string tradeTitle, bool isMysteryEgg, bool isFixOTRequest, bool isCloneRequest, bool isDumpRequest, bool isSpecialRequest, bool isBatchTrade, string pokemonDisplayName, bool isShiny)
    {
        string isPkmShiny = isShiny ? "Shiny " : "";
        return isMysteryEgg || isFixOTRequest || isCloneRequest || isDumpRequest || isSpecialRequest || isBatchTrade ?
               $"{username}'s {tradeTitle}" :
               $"{username}'s {isPkmShiny}{pokemonDisplayName}";
    }

    private static int CalculateMedals(int tradeCount)
    {
        int medals = 0;
        if (tradeCount >= 1) medals++;
        if (tradeCount >= 50) medals++;
        if (tradeCount >= 100) medals++;
        if (tradeCount >= 150) medals++;
        if (tradeCount >= 200) medals++;
        if (tradeCount >= 250) medals++;
        if (tradeCount >= 300) medals++;
        if (tradeCount >= 350) medals++;
        if (tradeCount >= 400) medals++;
        if (tradeCount >= 450) medals++;
        if (tradeCount >= 500) medals++;
        if (tradeCount >= 550) medals++;
        if (tradeCount >= 600) medals++;
        if (tradeCount >= 650) medals++;
        if (tradeCount >= 700) medals++;
        // Add more milestones if necessary
        return medals;
    }

    public static string GetUserDetails(int totalTradeCount, TradeCodeStorage.TradeCodeDetails? tradeDetails, string etaMessage, (int Position, int TotalBatchTrades) position)
    {
        // Initialize userDetailsText only if totalTradeCount > 0 and other conditions are met
        if (totalTradeCount > 0 && SysCord<T>.Runner.Config.Trade.TradeConfiguration.StoreTradeCodes && tradeDetails != null)
        {
            string userDetailsText = string.Empty;

            // Add ETA message first
            if (!string.IsNullOrEmpty(etaMessage))
            {
                userDetailsText += $"{etaMessage}\n"; // Add a newline after ETA message
            }

            // Check if "Current Queue Position" is not already included in etaMessage or userDetailsText
            if (!etaMessage.Contains("Current Queue Position") && !userDetailsText.Contains("Current Queue Position"))
            {
                userDetailsText += $"Current Queue Position: {(position.Position == -1 ? 1 : position.Position)}\n";
            }

            // Add Total Medals
            int totalMedals = CalculateMedals(totalTradeCount);
            // Add Total User Trades
            userDetailsText += $"Total User Trades: {totalTradeCount} | Medals: {totalMedals}\n";

            List<string> details = new List<string>();

            // Add Trainer Info, Use Null & 0 for Unknown Info
            if (!string.IsNullOrEmpty(tradeDetails?.OT))
            {
                details.Add($"OT: {tradeDetails?.OT}");
            }

            if (tradeDetails?.TID != 0)
            {
                details.Add($"TID: {tradeDetails?.TID}");
            }

            if (tradeDetails?.SID != 0)
            {
                details.Add($"SID: {tradeDetails?.SID}");
            }

            if (details.Count > 0)
            {
                userDetailsText += string.Join(" | ", details);
            }
            return userDetailsText;
        }
        return string.Empty;
    }



    public static void AddAdditionalText(EmbedBuilder embedBuilder)
    {
        string additionalText = string.Join("\n", SysCordSettings.Settings.AdditionalEmbedText);
        if (!string.IsNullOrEmpty(additionalText))
        {
            embedBuilder.AddField("\u200B", additionalText, inline: false);
        }
    }

    public static void AddNormalTradeFields(EmbedBuilder embedBuilder, EmbedData embedData, string trainerMention, T pk)
    {
        string leftSideContent = $"**Trainer:** {trainerMention}\n";

        leftSideContent +=

            (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowLevel ? $"**Level:** {embedData.Level}\n" : "") +
            (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowMetLevel ? $"**Met Level:** {embedData.MetLevel}\n" : "") +
            (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowMetDate ? $"**Met Date:** {embedData.MetDate}\n" : "") +
            (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowAbility ? $"**Ability:** {embedData.Ability}\n" : "") +
            (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowNature ? $"**Nature:** {embedData.Nature}\n" : "") +
            (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowLanguage ? $"**Language:** {GetLanguageText(embedData.Language)}\n" : "") +
            (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowIVs ? $"**IVs:** {embedData.IVsDisplay}\n" : "") +
            (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowEVs && !string.IsNullOrWhiteSpace(embedData.EVsDisplay) ? $"**EVs:** {embedData.EVsDisplay}\n" : "") +
            (pk.Version is GameVersion.SL or GameVersion.VL && SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowTeraType ? $"**Tera Type:** {embedData.TeraType}\n" : "") +
            (pk.Version is GameVersion.SL or GameVersion.VL && SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.ShowScale ? $"**Scale:** {embedData.Scale.Item1} ({embedData.Scale.Item2})\n" : "");

        leftSideContent = leftSideContent.TrimEnd('\n');
        embedBuilder.AddField($"**{embedData.SpeciesName}{(string.IsNullOrEmpty(embedData.FormName) ? "" : $"-{embedData.FormName}")} {embedData.SpecialSymbols}**", leftSideContent, inline: true);
        embedBuilder.AddField("\u200B", "\u200B", inline: true); // Spacer
        embedBuilder.AddField("**__MOVES__**", embedData.MovesDisplay, inline: true);
    }

    public static void AddSpecialTradeFields(EmbedBuilder embedBuilder, bool isMysteryEgg, bool isSpecialRequest, bool isCloneRequest, bool isFixOTRequest, string trainerMention)
    {
        string specialDescription = $"**Trainer:** {trainerMention}\n" +
                                    (isMysteryEgg ? "Mystery Egg" : isSpecialRequest ? "Special Request" : isCloneRequest ? "Clone Request" : isFixOTRequest ? "FixOT Request" : "Dump Request");
        embedBuilder.AddField("\u200B", specialDescription, inline: false);
    }

    public static void AddThumbnails(EmbedBuilder embedBuilder, bool isCloneRequest, bool isSpecialRequest, string heldItemUrl)
    {
        if (isCloneRequest || isSpecialRequest)
        {

        }
        else if (!string.IsNullOrEmpty(heldItemUrl))
        {
            embedBuilder.WithThumbnailUrl(heldItemUrl);
        }
    }

    private static int[] GetEVs(T pk)
    {
        int[] evs = new int[6];
        pk.GetEVs(evs);
        return evs;
    }
}

public class EmbedData
{
    public List<string>? Moves { get; set; }
    public int Level { get; set; }
    public string? TeraType { get; set; }
    public (string, byte) Scale { get; set; }
    public string? Ability { get; set; }
    public string? Nature { get; set; }
    public int Language { get; set; }
    public string? SpeciesName { get; set; }
    public string? SpecialSymbols { get; set; }
    public string? FormName { get; set; }
    public string? HeldItem { get; set; }
    public string? Ball { get; set; }
    public string? IVsDisplay { get; set; }
    public string? EVsDisplay { get; set; }
    public string? MetDate { get; set; }
    public byte MetLevel { get; set; }
    public string? MovesDisplay { get; set; }
    public string? PokemonDisplayName { get; set; }
    public string? TradeTitle { get; set; }
    public string? AuthorName { get; set; }
    public string? EmbedImageUrl { get; set; }
    public string? HeldItemUrl { get; set; }
    public bool IsLocalFile { get; set; }
}
