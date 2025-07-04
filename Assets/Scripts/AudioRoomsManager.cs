using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StreamVideo.Core;
using StreamVideo.Core.StatefulModels;
using StreamVideo.Libs.Auth;
using UnityEngine;
using UnityEngine.Android;

public class AudioRoomsManager : MonoBehaviour
{
    
    // api key, user id and the user token - essential to connect a User to the Stream Video API.
    [SerializeField]
    private string _apiKey; // you'd obtain the api key assigned to your application from the Stream's Dashboard.

    [SerializeField]
    private string
        _userId; //  The user id and the user token would typically be generated by your backend service using one of our server-side SDKs.

    [SerializeField] private string _userToken;

    public IStreamVideoClient StreamClient { get; private set; }
    private IStreamCall _activeCall;
    
    public event Action<IStreamVideoCallParticipant> ParticipantJoined;
    public event Action<string> ParticipantLeft;

    protected async void Awake()
    {
        // Request microphone permissions first
        Permission.RequestUserPermission(Permission.Microphone);

        // Wait until the user responds
        while (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.Log("Waiting for microphone permission...");
            await Task.Delay(2000);
        }
        
        // Create Client instance
        StreamClient = StreamVideoClient.CreateDefaultClient();
        var credentials = new AuthCredentials(_apiKey, _userId, _userToken);

        try
        {
            // Connect user to Stream server
            await StreamClient.ConnectUserAsync(credentials);
            Debug.Log($"User `{_userId}` is connected to Stream server");
        }
        catch (Exception e)
        {
            // Log potential issues that occured during trying to connect
            Debug.LogException(e);
        }
    }

    public async Task JoinCallAsync(string callId)
    {
        _activeCall = await StreamClient.JoinCallAsync(StreamCallType.Default, callId, create: true, ring: false, notify: false);

        // Handle already present participants
        foreach (var participant in _activeCall.Participants)
        {
            OnParticipantJoined(participant);
        }

        // Subscribe to events in order to react to participant joining or leaving the call
        _activeCall.ParticipantJoined += OnParticipantJoined;
        _activeCall.ParticipantLeft += OnParticipantLeft;
    }

    public async Task LeaveCallAsync()
    {
        if (_activeCall == null)
        {
            Debug.LogWarning("Leave request ignored. There is no active call to leave.");
            return;
        }
    
        // Unsubscribe from events 
        _activeCall.ParticipantJoined -= OnParticipantJoined;
        _activeCall.ParticipantLeft -= OnParticipantLeft;
    
        await _activeCall.LeaveAsync();
    }
    
    private void OnParticipantJoined(IStreamVideoCallParticipant participant)
    {
        ParticipantJoined?.Invoke(participant);
    }

    private void OnParticipantLeft(string sessionId, string userid)
    {
        ParticipantLeft?.Invoke(sessionId);
    }
}