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

    static void CheckString( Package p, int resId, int cultureId, object expectedValue, object expectedMatchedCultureId )
    {
        p.Database.ExecuteScalar( "select Value from CK.vMCResHtml where ResId=@0 and CultureId = @1", resId, cultureId )
            .ShouldBe( expectedValue );
        p.Database.ExecuteScalar( "select MatchedCultureId from CK.vMCResHtml where ResId=@0 and CultureId = @1", resId, cultureId )
            .ShouldBe( expectedMatchedCultureId );
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
