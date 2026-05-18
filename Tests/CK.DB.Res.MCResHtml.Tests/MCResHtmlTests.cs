using CK.Core;
using CK.SqlServer;
using CK.Testing;
using Shouldly;
using NUnit.Framework;
using System;

namespace CK.DB.Res.MCResHtml.Tests;

[TestFixture]
public class MCResHtmlTests
{
    // CultureIds from CK.DB.Globalization.
    const int EnCultureId = 221277614;  // "en"
    const int FrCultureId = 210333265;  // "fr"
    const int DeCultureId = 223899012;  // "de"
    // Test-only CultureIds (registered on the fly under fr and de).
    const int FrCaCultureId = 1621867518;
    const int DeAtCultureId = 990000002;
    // Test-only orphan culture (no link to English): used to validate the
    // ultimate English fallback that the view applies regardless of chain.
    const int OrphanCultureId = 990000003;

    [Test]
    public void fallbaks_between_french_and_english_cultures()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int noValuesId, enId, frId, bothId;
            AssumeFallbackTestEnglishAndFrenchResources( p, ctx, out noValuesId, out enId, out frId, out bothId );

            CheckString( p, noValuesId, EnCultureId, DBNull.Value, DBNull.Value );
            CheckString( p, noValuesId, FrCultureId, DBNull.Value, DBNull.Value );

            CheckString( p, enId, EnCultureId, "Only in English.", EnCultureId );
            CheckString( p, enId, FrCultureId, "Only in English.", EnCultureId );

            // fr's parent is en; asking en when only fr is set has no fallback (en is root).
            CheckString( p, frId, EnCultureId, DBNull.Value, DBNull.Value );
            CheckString( p, frId, FrCultureId, "Seulement en Français.", FrCultureId );

            CheckString( p, bothId, EnCultureId, "English (and French).", EnCultureId );
            CheckString( p, bothId, FrCultureId, "Français (et Anglais).", FrCultureId );
        }
    }

    [Test]
    public void fallbaks_between_french_and_english_and_german_cultures()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {

            int noValuesId, enId, frId, bothId;
            AssumeFallbackTestEnglishAndFrenchResources( p, ctx, out noValuesId, out enId, out frId, out bothId );
            int deId = p.ResTable.Create( ctx );
            int allId = p.ResTable.Create( ctx );

            RegisterGerman( p, ctx );

            p.MCResHtmlTable.SetHtml( ctx, deId, DeCultureId, "Nur in deutscher Sprache." );
            p.MCResHtmlTable.SetHtml( ctx, allId, FrCultureId, "Français (et Anglais et Allemand)." );
            p.MCResHtmlTable.SetHtml( ctx, allId, EnCultureId, "English (and French and German)." );
            p.MCResHtmlTable.SetHtml( ctx, allId, DeCultureId, "Deutsch (und Englisch und Französisch)." );

            CheckString( p, noValuesId, EnCultureId, DBNull.Value, DBNull.Value );
            CheckString( p, noValuesId, FrCultureId, DBNull.Value, DBNull.Value );
            CheckString( p, noValuesId, DeCultureId, DBNull.Value, DBNull.Value );

            CheckString( p, enId, EnCultureId, "Only in English.", EnCultureId );
            CheckString( p, enId, FrCultureId, "Only in English.", EnCultureId );
            CheckString( p, enId, DeCultureId, "Only in English.", EnCultureId );

            // en is root so no fallback. de's chain is de -> en (fr not reachable from de).
            CheckString( p, frId, EnCultureId, DBNull.Value, DBNull.Value );
            CheckString( p, frId, FrCultureId, "Seulement en Français.", FrCultureId );
            CheckString( p, frId, DeCultureId, DBNull.Value, DBNull.Value );

            CheckString( p, bothId, EnCultureId, "English (and French).", EnCultureId );
            CheckString( p, bothId, FrCultureId, "Français (et Anglais).", FrCultureId );
            CheckString( p, bothId, DeCultureId, "English (and French).", EnCultureId );

            CheckString( p, allId, EnCultureId, "English (and French and German).", EnCultureId );
            CheckString( p, allId, FrCultureId, "Français (et Anglais et Allemand).", FrCultureId );
            CheckString( p, allId, DeCultureId, "Deutsch (und Englisch und Französisch).", DeCultureId );
        }
    }

    static void AssumeFallbackTestEnglishAndFrenchResources( Package p, SqlStandardCallContext ctx, out int noValuesId, out int enId, out int frId, out int bothId )
    {
        noValuesId = p.ResTable.Create( ctx );
        enId = p.ResTable.Create( ctx );
        p.MCResHtmlTable.SetHtml( ctx, enId, EnCultureId, "Only in English." );
        frId = p.ResTable.Create( ctx );
        p.MCResHtmlTable.SetHtml( ctx, frId, FrCultureId, "Seulement en Français." );
        bothId = p.ResTable.Create( ctx );
        p.MCResHtmlTable.SetHtml( ctx, bothId, EnCultureId, "English (and French)." );
        p.MCResHtmlTable.SetHtml( ctx, bothId, FrCultureId, "Français (et Anglais)." );
    }

    static void RegisterGerman( Package p, SqlStandardCallContext ctx )
    {
        p.Database.ExecuteNonQuery(
            "if not exists (select 1 from CK.tCulture where CultureId = @0) exec CK.sCultureRegister @0, @1, @2, @3, @4, @5, @6, @7;",
            DeCultureId, "de", "de", "German", "Deutsch", "German", 1, EnCultureId );
    }

    static void RegisterRegionalCultures( Package p, SqlStandardCallContext ctx )
    {
        RegisterGerman( p, ctx );
        p.Database.ExecuteNonQuery(
            "if not exists (select 1 from CK.tCulture where CultureId = @0) exec CK.sCultureRegister @0, @1, @2, @3, @4, @5, @6, @7;",
            FrCaCultureId, "fr-CA", "fr-CA", "French (Canada)", "français (Canada)", "French (Canada)", 0, FrCultureId );
        p.Database.ExecuteNonQuery(
            "if not exists (select 1 from CK.tCulture where CultureId = @0) exec CK.sCultureRegister @0, @1, @2, @3, @4, @5, @6, @7;",
            DeAtCultureId, "de-AT", "de-AT", "German (Austria)", "Deutsch (Österreich)", "German (Austria)", 0, DeCultureId );
    }

    static void CheckString( Package p, int resId, int cultureId, object expectedValue, object expectedMatchedCultureId )
    {
        p.Database.ExecuteScalar( "select Value from CK.vMCResHtml where ResId=@0 and CultureId = @1", resId, cultureId )
            .ShouldBe( expectedValue );
        p.Database.ExecuteScalar( "select MatchedCultureId from CK.vMCResHtml where ResId=@0 and CultureId = @1", resId, cultureId )
            .ShouldBe( expectedMatchedCultureId );
    }

    [Test]
    public void setting_and_clearing_values_traverses_culture_hierarchy()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            RegisterRegionalCultures( p, ctx );
            int resId = p.ResTable.Create( ctx );

            // No value: every culture in the hierarchy resolves to null.
            CheckString( p, resId, EnCultureId, DBNull.Value, DBNull.Value );
            CheckString( p, resId, FrCultureId, DBNull.Value, DBNull.Value );
            CheckString( p, resId, FrCaCultureId, DBNull.Value, DBNull.Value );
            CheckString( p, resId, DeCultureId, DBNull.Value, DBNull.Value );
            CheckString( p, resId, DeAtCultureId, DBNull.Value, DBNull.Value );

            // Value at the root: every descendant culture falls back to it.
            p.MCResHtmlTable.SetHtml( ctx, resId, EnCultureId, "English root." );
            CheckString( p, resId, EnCultureId, "English root.", EnCultureId );
            CheckString( p, resId, FrCultureId, "English root.", EnCultureId );
            CheckString( p, resId, FrCaCultureId, "English root.", EnCultureId );
            CheckString( p, resId, DeCultureId, "English root.", EnCultureId );
            CheckString( p, resId, DeAtCultureId, "English root.", EnCultureId );

            // Value at mid level: fr and fr-CA see fr; de branch keeps en.
            p.MCResHtmlTable.SetHtml( ctx, resId, FrCultureId, "Français." );
            CheckString( p, resId, EnCultureId, "English root.", EnCultureId );
            CheckString( p, resId, FrCultureId, "Français.", FrCultureId );
            CheckString( p, resId, FrCaCultureId, "Français.", FrCultureId );
            CheckString( p, resId, DeCultureId, "English root.", EnCultureId );
            CheckString( p, resId, DeAtCultureId, "English root.", EnCultureId );

            // Value at leaf: only fr-CA sees it; fr stays on its own value.
            p.MCResHtmlTable.SetHtml( ctx, resId, FrCaCultureId, "Québécois." );
            CheckString( p, resId, EnCultureId, "English root.", EnCultureId );
            CheckString( p, resId, FrCultureId, "Français.", FrCultureId );
            CheckString( p, resId, FrCaCultureId, "Québécois.", FrCaCultureId );

            // Value on a parallel branch: de and de-AT see de, fr branch is unchanged.
            p.MCResHtmlTable.SetHtml( ctx, resId, DeCultureId, "Deutsch." );
            CheckString( p, resId, FrCultureId, "Français.", FrCultureId );
            CheckString( p, resId, FrCaCultureId, "Québécois.", FrCaCultureId );
            CheckString( p, resId, DeCultureId, "Deutsch.", DeCultureId );
            CheckString( p, resId, DeAtCultureId, "Deutsch.", DeCultureId );

            // Clearing the leaf restores the fallback through the parent chain.
            p.MCResHtmlTable.SetHtml( ctx, resId, FrCaCultureId, null );
            CheckString( p, resId, FrCaCultureId, "Français.", FrCultureId );

            // Clearing the mid level cascades fr-CA back to the root.
            p.MCResHtmlTable.SetHtml( ctx, resId, FrCultureId, null );
            CheckString( p, resId, FrCultureId, "English root.", EnCultureId );
            CheckString( p, resId, FrCaCultureId, "English root.", EnCultureId );

            // Destroying the resource clears every culture entry.
            Assert.DoesNotThrow( () => p.ResTable.Destroy( ctx, resId ) );
            CheckString( p, resId, EnCultureId, null, null );
            CheckString( p, resId, FrCultureId, null, null );
            CheckString( p, resId, FrCaCultureId, null, null );
            CheckString( p, resId, DeCultureId, null, null );
            CheckString( p, resId, DeAtCultureId, null, null );
        }
    }

    [Test]
    public void english_is_the_ultimate_fallback_for_cultures_not_linked_to_english()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            // Register an orphan root culture: no ParentCultureId so its chain does NOT include English.
            p.Database.ExecuteNonQuery(
                "if not exists (select 1 from CK.tCulture where CultureId = @0) exec CK.sCultureRegister @0, @1, @2, @3, @4, @5, @6, null;",
                OrphanCultureId, "zz", "zz", "Orphan", "Orphan", "Orphan", 1 );

            int resId = p.ResTable.Create( ctx );

            // No value anywhere: orphan resolves to null.
            CheckString( p, resId, OrphanCultureId, DBNull.Value, DBNull.Value );

            // Only English is set: the orphan, although unrelated, falls back to English.
            p.MCResHtmlTable.SetHtml( ctx, resId, EnCultureId, "English value." );
            CheckString( p, resId, EnCultureId, "English value.", EnCultureId );
            CheckString( p, resId, OrphanCultureId, "English value.", EnCultureId );

            // A value set on the orphan itself wins over the English fallback.
            p.MCResHtmlTable.SetHtml( ctx, resId, OrphanCultureId, "Orphan value." );
            CheckString( p, resId, OrphanCultureId, "Orphan value.", OrphanCultureId );
            CheckString( p, resId, EnCultureId, "English value.", EnCultureId );

            // Clearing the orphan-specific value restores the English fallback.
            p.MCResHtmlTable.SetHtml( ctx, resId, OrphanCultureId, null );
            CheckString( p, resId, OrphanCultureId, "English value.", EnCultureId );

            p.ResTable.Destroy( ctx, resId );
        }
    }

    [Test]
    public void destroying_the_resource_destroys_the_string_values()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            int noValuesId, enId, frId, bothId;
            AssumeFallbackTestEnglishAndFrenchResources( p, ctx, out noValuesId, out enId, out frId, out bothId );

            p.ResTable.Destroy( ctx, noValuesId );
            p.ResTable.Destroy( ctx, enId );
            p.ResTable.Destroy( ctx, frId );
            p.ResTable.Destroy( ctx, bothId );
            p.Database.ExecuteReader( "select Value from CK.vMCResHtml where ResId=@0", bothId )
                .Rows.ShouldBeEmpty();

            Assert.DoesNotThrow( () => p.MCResHtmlTable.SetHtml( ctx, bothId, EnCultureId, null ) );
        }

    }

}
