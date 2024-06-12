using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.User.Security.Types;
using OrderingApplication.Features.User.Utilities;
using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.User.Security;

internal sealed class AccountSecurityManager( AccountConfig accountConfig, UserManager<UserAccount> userManager )
{
    readonly AccountConfig _config = accountConfig;
    readonly UserManager<UserAccount> _userManager = userManager;

    internal async Task<Reply<ViewSecurityResponse>> GetSecuritySettings( string userId )
    {
        Reply<UserAccount> userReply = await _userManager.FindById( userId );
        return userReply.Succeeded
            ? Reply<ViewSecurityResponse>.Success( new ViewSecurityResponse( userReply.Data.TwoFactorEnabled, userReply.Data.TwoFactorEmail ) )
            : Reply<ViewSecurityResponse>.Failure( "User not found." );
    }
    internal async Task<Reply<bool>> UpdatePassword( string userId, UpdatePasswordRequest request )
    {
        Reply<UserAccount> userReply = await _userManager.FindById( userId );
        return userReply.Succeeded
            ? await TryUpdatePassword( userReply.Data, request )
            : IReply.Fail( "User not found." );
    }
    internal async Task<Reply<bool>> Update2Fa( string userId, Update2FaRequest request )
    {
        Reply<UserAccount> userReply = await _userManager.FindById( userId );
        if (!userReply.Succeeded)
            return Reply<bool>.Failure( "User not found." );
        
        UserAccount user = userReply.Data;

        if (request.TwoFactorEmail == user.Email)
            return IReply.Fail( "Two Factor Email cannot be the same as your primary email." );
        
        user.TwoFactorEnabled = request.IsEnabled;
        user.TwoFactorEmail = request.TwoFactorEmail;

        IdentityResult updateResult = await _userManager.UpdateAsync( user );
        return updateResult.Succeeded
            ? IReply.Success()
            : IReply.Fail( "Failed to save changes to two factor." );
    }

    async Task<Reply<bool>> TryUpdatePassword( UserAccount user, UpdatePasswordRequest request )
    {
        var changed = (await _userManager.ChangePasswordAsync( user, request.OldPassword, request.NewPassword ))
            .Succeeds( out IdentityResult result );

        return changed
            ? IReply.Success()
            : IReply.Fail( result.CombineErrors() );
    }
}