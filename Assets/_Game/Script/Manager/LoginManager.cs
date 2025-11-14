using Facebook.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

public class LoginManager : Singleton<LoginManager>
{
    private async void Awake()
    {
        InitializeFacebook();
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Debug.Log("Services Initializing");
            await UnityServices.InitializeAsync();
        }

        PlayerAccountService.Instance.SignedIn += SignInOrLinkWithUnity;
    }

    // Start is called before the first frame update
    private async void Start()
    {
        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            Debug.Log("Session Token not found");
            return;
        }

        Debug.Log("Returning player sigining in ...");
        await SignInAnonymouslyAsync();
    }

    #region Login
    public async void StartUnitySignInAsync()
    {
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            SignInOrLinkWithUnity();
            return;
        }

        try
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    private async void SignInOrLinkWithUnity()
    {
        try
        {
            // 1. Player is not yet authenticated, signing up with Unity
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Signing up with Unity Player Account ... ");
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Successfully signed up with Unity Player Account");
                return;
            }

            // 2. Player is authenticated, but does not yet have a Unity ID linked, so let's link
            if (!HasUnityID())
            {
                Debug.Log("Linking anonymous account to Unity ... ");
                await LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Successfully linked anonymous account!");
                return;
            }

            // 3. Player has authentication and a Unity ID
            Debug.Log("Player is already signed in to their Unity Player Account");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    private bool HasUnityID()
    {
        return AuthenticationService.Instance.PlayerInfo.GetUnityId() != null;
    }

    #region AnonymusLogin
    public async void StartAnonymousSignIn()
    {
        await SignInAnonymouslyAsync();
    }

    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
    #endregion

    #region FacebookLogin
    public void StartFacebookSignIn()
    {
        var perms = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(perms, async result =>
        {
            if (FB.IsLoggedIn)
            {
                // AccessToken class will have session details
                var facebookAccessToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString;

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await SignInWithFacebookAsync(facebookAccessToken);
                }
                else
                {
                    await LinkWithFacebookAsync(facebookAccessToken);
                }
            }
            else
            {
                Debug.Log("User cancelled login");
            }
        });
    }

    private async Task SignInWithFacebookAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithFacebookAsync(accessToken);
            Debug.Log("Signed in with Facebook!");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    private void InitializeFacebook()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }
    #endregion

    #endregion

    #region Link
    private async Task LinkWithUnityAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
            Debug.Log("Link is successful.");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    private async Task LinkWithFacebookAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithFacebookAsync(accessToken);
            Debug.Log("Linked with Facebook!");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            Debug.LogException(ex);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion

    #region Unlink/Delete/SignOut
    public void SignOut()
    {
        Debug.Log("Signing out ... ");

        AuthenticationService.Instance.SignOut();
        PlayerAccountService.Instance.SignOut();
    }

    public async void UnlinkUnity()
    { 
        try
        {
            Debug.Log("Unlinking Unity Player Account ... ");
            await AuthenticationService.Instance.UnlinkUnityAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void ClearSessionToken()
    {
        Debug.Log("Clearing session token ... ");

        AuthenticationService.Instance.ClearSessionToken();
    }

    public void DeleteAccount()
    {
        Debug.Log("Deleting account ... ");

        AuthenticationService.Instance.DeleteAccountAsync();
    }
    #endregion
}
