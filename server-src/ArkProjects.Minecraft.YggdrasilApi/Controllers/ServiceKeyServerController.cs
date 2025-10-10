using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.Database.Entities;
using ArkProjects.Minecraft.YggdrasilApi.Filters;
using ArkProjects.Minecraft.YggdrasilApi.Models.ServiceServer;
using ArkProjects.Minecraft.YggdrasilApi.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArkProjects.Minecraft.YggdrasilApi.Controllers;

[ApiController]
[Route("service/key")]
[ServiceAuth(RequireCreateOtherService = true)]
public class ServiceKeyServerController(
    ILogger<ServiceKeyServerController> logger,
    IDbContextFactory<McDbContext> contextFactory,
    IServiceServerService servicesService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ServiceEntity>>> GetList(CancellationToken ct = default)
    {
        return Ok(await servicesService.GetServicesAsync(ct));
    }

    [HttpGet("{serviceName}")]
    public async Task<ActionResult<ServiceEntity>> Get([FromRoute] string serviceName, CancellationToken ct = default)
    {
        ServiceEntity? service = await servicesService.GetServiceAsync(serviceName, ct);
        if (service == null)
            return NotFound(new { detail = "Service not found" });
        return Ok(service);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceEntity>> New([FromBody] ServiceCreateRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { detail = "Service name required" });

        await using McDbContext context = await contextFactory.CreateDbContextAsync(ct);
        if (await context.Services.AnyAsync(x => x.Name == req.Name, ct))
            return Conflict(new { detail = "Service already exists" });

        ServiceEntity service = await servicesService.CreateServiceAsync(req.Name, req.CreateOtherService, ct);
        logger.LogInformation($"Created new service {req.Name}");
        return Created($"service/key/{service.Name}", service);
    }

    [HttpDelete("{serviceName}")]
    public async Task<ActionResult> Remove([FromRoute] string serviceName, CancellationToken ct = default)
    {
        await using McDbContext context = await contextFactory.CreateDbContextAsync(ct);
        if (!await servicesService.DeleteServiceAsync(serviceName, ct))
            return NotFound(new { detail = "Service not found" });
        logger.LogInformation($"Deleted service {serviceName}");
        return NoContent();
    }

    [HttpPut("{serviceName}")]
    public async Task<ActionResult<ServiceEntity>> Put([FromRoute] string serviceName, [FromBody] ServicePutRequest req, CancellationToken ct = default)
    {
        ServiceEntity? service = await servicesService.UpdateServiceAsync(serviceName, req.CreateOtherService, ct);
        if (service == null)
            return NotFound(new { detail = "Service not found" });

        logger.LogInformation($"Updated service {service.Name}");
        return Ok(service);
    }
}