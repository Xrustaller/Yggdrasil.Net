using System.ComponentModel.DataAnnotations;

namespace ArkProjects.Minecraft.Database.Entities;

public class ServiceEntity
{
    [Key] public string Name { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;
    public bool CreateOtherService { get; set; } = false;
}