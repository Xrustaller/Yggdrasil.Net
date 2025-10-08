using ArkProjects.Minecraft.YggdrasilApi.Models;

namespace ArkProjects.Minecraft.YggdrasilApi.Misc;

public class YgServerException(ErrorResponse response, Exception? innerException = null) : Exception(response.ErrorMessage, innerException)
{
    public ErrorResponse Response { get; } = response;
}