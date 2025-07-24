using UnityEngine;

public static class CommandInputManager
{
    private static object currentOwner;

    public static bool TryClaim(object requester)
    {
        if (currentOwner == null || currentOwner == requester)
        {
            currentOwner = requester;
            return true;
        }
        return false;
    }

    public static void Release(object requester)
    {
        if (currentOwner == requester)
            currentOwner = null;
    }
}
