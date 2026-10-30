using CK.Core;
using CK.Cris;
using CK.IO.User.UserOidc;
using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CK.DB.User.UserOidc;

public partial class Package
{
    /// <summary>
    /// Lists the OpenID Connect identities of the current user.
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="cmd">The command.</param>
    /// <param name="table">The identities table.</param>
    /// <param name="pocoDirectory">The poco directory.</param>
    /// <returns>The identities of the current user.</returns>
    [CommandHandler]
    public virtual Task<List<IUserOidcIdentity>> HandleGetOidcIdentitiesAsync( ISqlCallContext ctx, IGetOidcIdentitiesQCommand cmd, UserOidcTable table, PocoDirectory pocoDirectory )
    {
        return table.ReadUserIdentitiesAsync( ctx, pocoDirectory, cmd.ActorId.GetValueOrDefault() );
    }

    /// <summary>
    /// Unlinks an OpenID Connect identity of the current user. The user's last way to log in is never unlinked.
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="collector">The message collector.</param>
    /// <param name="cmd">The command.</param>
    /// <param name="table">The identities table.</param>
    /// <returns>The result.</returns>
    [CommandHandler]
    public virtual async Task<ICrisBasicCommandResult> HandleUnlinkOidcIdentityAsync( ISqlCallContext ctx, UserMessageCollector collector, IUnlinkOidcIdentityCommand cmd, UserOidcTable table )
    {
        var res = cmd.CreateResult();
        var actorId = cmd.ActorId.GetValueOrDefault();
        try
        {
            switch( await table.UnlinkIdentityAsync( ctx, actorId, cmd.SchemeSuffix ) )
            {
                case OidcUnlinkResult.Unlinked:
                    ctx.Monitor.Info( ActivityMonitor.Tags.SecurityCritical, $"OpenID Connect identity '{cmd.SchemeSuffix}' of user #{actorId} unlinked." );
                    collector.Info( "The account has been unlinked.", "Oidc.Unlinked" );
                    break;
                case OidcUnlinkResult.LastLoginMethod:
                    collector.Error( "This account is your last way to log in: it cannot be unlinked.", "Oidc.LastLoginMethod" );
                    break;
                default:
                    collector.Error( "This account is not linked.", "Oidc.NotFound" );
                    break;
            }
        }
        catch( Exception ex )
        {
            ctx.Monitor.Error( $"Error while unlinking the OpenID Connect identity '{cmd.SchemeSuffix}' of user #{actorId}.", ex );
            collector.Error( "An error occurred while unlinking the account.", "Oidc.UnlinkFailed" );
        }
        res.SetUserMessages( collector );
        return res;
    }
}
