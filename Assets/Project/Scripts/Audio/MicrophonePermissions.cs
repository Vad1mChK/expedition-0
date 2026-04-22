#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine;

namespace Expedition0.Audio
{
    public static class MicrophonePermissions
    {
        public static void EnsureMicrophonePermissionAndroid()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                Permission.RequestUserPermission(Permission.Microphone);
#endif
        }
    }
}