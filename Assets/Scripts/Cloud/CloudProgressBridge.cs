using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if IDLESPACE_UGS
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
using Unity.Services.Core;
#endif

public class CloudProgressBridge : MonoBehaviour
{
    private const string PendingProgressKey = "idle_progress_pending";

    [SerializeField] private bool enableCloudBackend = true;
    [SerializeField] private string loadProgressEndpoint = "idle_load_progress";
    [SerializeField] private string saveProgressEndpoint = "idle_save_progress";

    public bool IsReady { get; private set; }
    public string PlayerId { get; private set; }

    public async Task<bool> InitializeAndSignInAnonymousAsync()
    {
        if (string.IsNullOrWhiteSpace(loadProgressEndpoint) || string.IsNullOrWhiteSpace(saveProgressEndpoint))
        {
            Debug.LogWarning("Unity cloud endpoints are not configured.");
            return false;
        }

        if (!enableCloudBackend)
        {
            return false;
        }

#if IDLESPACE_UGS
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            IsReady = AuthenticationService.Instance.IsSignedIn;
            PlayerId = AuthenticationService.Instance.PlayerId;
            return IsReady;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Unity cloud sign-in failed. Local save will be used. " + exception.Message);
            IsReady = false;
            return false;
        }
#else
        await Task.CompletedTask;
        Debug.LogWarning("Unity cloud backend is disabled. Install UGS packages and add IDLESPACE_UGS to Scripting Define Symbols.");
        return false;
#endif
    }

    public async Task<bool> SignInWithCustomTokensAsync(string accessToken, string sessionToken)
    {
        if (!enableCloudBackend || string.IsNullOrEmpty(accessToken))
        {
            return false;
        }

#if IDLESPACE_UGS
        try
        {
            await UnityServices.InitializeAsync();
            AuthenticationService.Instance.ProcessAuthenticationTokens(accessToken, sessionToken);
            await Task.Yield();

            IsReady = AuthenticationService.Instance.IsSignedIn;
            PlayerId = AuthenticationService.Instance.PlayerId;
            return IsReady;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Unity custom sign-in failed. " + exception.Message);
            IsReady = false;
            return false;
        }
#else
        await Task.CompletedTask;
        return false;
#endif
    }

    public async Task<bool> SignInWithPlatformProviderAsync(Func<Task> platformSignInOperation)
    {
        if (!enableCloudBackend || platformSignInOperation == null)
        {
            return false;
        }

#if IDLESPACE_UGS
        try
        {
            await UnityServices.InitializeAsync();
            await platformSignInOperation.Invoke();

            IsReady = AuthenticationService.Instance.IsSignedIn;
            PlayerId = AuthenticationService.Instance.PlayerId;
            return IsReady;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Unity platform sign-in failed. " + exception.Message);
            IsReady = false;
            return false;
        }
#else
        await Task.CompletedTask;
        return false;
#endif
    }

    public async Task<GameProgressSnapshot> LoadProgressAsync()
    {
        if (!IsReady)
        {
            return null;
        }

#if IDLESPACE_UGS
        try
        {
            LoadProgressResponse response = await CloudCodeService.Instance.CallEndpointAsync<LoadProgressResponse>(
                loadProgressEndpoint,
                new Dictionary<string, object>());

            if (response == null || !response.hasProgress || string.IsNullOrEmpty(response.progressJson))
            {
                return null;
            }

            GameProgressSnapshot snapshot = JsonUtility.FromJson<GameProgressSnapshot>(response.progressJson);

            if (snapshot != null && snapshot.clientSavedAtUnixTime <= 0)
            {
                snapshot.clientSavedAtUnixTime = snapshot.savedAtUnixTime;
            }

            if (snapshot != null && snapshot.savedAtUnixTime <= 0)
            {
                snapshot.savedAtUnixTime = response.serverUnixTime > 0
                    ? response.serverUnixTime
                    : snapshot.savedAtUnixTime;

                if (snapshot.clientSavedAtUnixTime <= 0)
                {
                    snapshot.clientSavedAtUnixTime = snapshot.savedAtUnixTime;
                }
            }

            return snapshot;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Unity cloud progress load failed. Local save will be used. " + exception.Message);
            return null;
        }
#else
        await Task.CompletedTask;
        return null;
#endif
    }

    public async Task<bool> SaveProgressAsync(GameProgressSnapshot snapshot)
    {
        if (!IsReady || snapshot == null)
        {
            return false;
        }

#if IDLESPACE_UGS
        try
        {
            snapshot.schemaVersion = GameProgressSnapshot.CurrentSchemaVersion;
            string progressJson = JsonUtility.ToJson(snapshot);
            Dictionary<string, object> pendingProgress = new Dictionary<string, object>
            {
                { PendingProgressKey, progressJson }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(pendingProgress);

            SaveProgressResponse response = await CloudCodeService.Instance.CallEndpointAsync<SaveProgressResponse>(
                saveProgressEndpoint,
                new Dictionary<string, object>());

            if (response != null && response.savedAtUnixTime > 0)
            {
                snapshot.savedAtUnixTime = response.savedAtUnixTime;
                snapshot.clientSavedAtUnixTime = response.savedAtUnixTime;
            }
            else if (response != null && response.serverUnixTime > 0 && snapshot.savedAtUnixTime <= 0)
            {
                snapshot.savedAtUnixTime = response.serverUnixTime;

                if (snapshot.clientSavedAtUnixTime <= 0)
                {
                    snapshot.clientSavedAtUnixTime = snapshot.savedAtUnixTime;
                }
            }

            return response == null || response.saved;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Unity cloud progress save failed. Local save remains available. " + exception.Message);
            return false;
        }
#else
        await Task.CompletedTask;
        return false;
#endif
    }

    public void SignOut(bool clearCredentials)
    {
#if IDLESPACE_UGS
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut(clearCredentials);
        }
#endif

        IsReady = false;
        PlayerId = string.Empty;
    }

    [Serializable]
    private class LoadProgressResponse
    {
        public bool hasProgress = false;
        public string progressJson = string.Empty;
        public long serverUnixTime = 0;
    }

    [Serializable]
    private class SaveProgressResponse
    {
        public bool saved = false;
        public long savedAtUnixTime = 0;
        public long serverUnixTime = 0;
    }
}
