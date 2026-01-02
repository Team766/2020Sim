using UnityEngine;
using System;
using System.Collections.Generic;
using System.Web;

public enum PlayerRole
{
    INPUT,
    CODE,
    CODE_AND_INPUT,
}

public static class PlayerRoleExtensions
{
    public static bool IsInputPlayer(this PlayerRole role)
    {
        return role switch
        {
            PlayerRole.INPUT => true,
            PlayerRole.CODE_AND_INPUT => true,
            PlayerRole.CODE => false,
            _ => throw new ArgumentOutOfRangeException($"Unknown PlayerRole value: {role}"),
        };
    }

    public static bool IsCodePlayer(this PlayerRole role)
    {
        return role switch
        {
            PlayerRole.INPUT => false,
            PlayerRole.CODE_AND_INPUT => true,
            PlayerRole.CODE => true,
            _ => throw new ArgumentOutOfRangeException($"Unknown PlayerRole value: {role}"),
        };
    }
}

public static class ApplicationArguments
{
    public static readonly PlayerRole PlayerRole = GetArgument<PlayerRole>("playerRole", editorValue: PlayerRole.CODE_AND_INPUT);

    public static readonly string AuthPublicKey = GetArgument("robotPublicKey", editorValue: "thePublicKey");
    public static readonly string AuthPrivateKey = GetArgument("robotPrivateKey", editorValue: "itsSecret");

    private static T GetArgument<T>(string argumentName, T editorValue)
    {
#if UNITY_EDITOR
        return editorValue;
#else
        return GetArgument<T>(argumentName);
#endif
    }

    private static string GetArgument(string argumentName)
    {
#if UNITY_WEBGL
        var uri = new Uri(Application.absoluteURL);
        return HttpUtility.ParseQueryString(uri.Query).Get(argumentName);
#else
        string search = "--" + argumentName + "=";
        foreach (var arg in Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith(search))
            {
                return arg.Substring(search.Length);
            }
        }
        return null;
#endif
    }

    private static T GetArgument<T>(string argumentName) where T : struct, Enum
    {
        return Enum.Parse<T>(GetArgument(argumentName), true);
    }
}
