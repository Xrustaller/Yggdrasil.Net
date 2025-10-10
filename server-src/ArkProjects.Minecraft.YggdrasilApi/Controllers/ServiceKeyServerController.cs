using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
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
    IServiceServerService servicesService) : ControllerBase
{
    //[HttpPost("calc/{serviceName}")]
    //public async Task<IActionResult> Calc([FromRoute]string serviceName, [FromBody] CalcBody body, CancellationToken ct = default)
    //{
    //    long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    //    string timestamp = ts.ToString();
    //    string payload = $"{serviceName}:{timestamp}";
    //    using HMACSHA256 hmac = new(Convert.FromBase64String(body.Secret));
    //    string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    //    Dictionary<string, string> result = new()
    //    {
    //        {"X-Service-Name", serviceName},
    //        {"X-Timestamp", timestamp},
    //        {"X-Signature", signature}
    //    };
    //    return Ok(result);
    //}
    //public class CalcBody()
    //{
    //    [JsonPropertyName("secret")]
    //    public string Secret { get; set; }
    //}
    
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
        
        ServiceEntity? service = await servicesService.CreateServiceAsync(req.Name, req.CreateOtherService, ct);
        
        if (service == null)
            return Conflict(new { detail = "Service already exists" });
        
        logger.LogInformation($"Created new service {req.Name}");
        return Created($"service/key/{service.Name}", service);
    }

    [HttpDelete("{serviceName}")]
    public async Task<ActionResult> Remove([FromRoute] string serviceName, CancellationToken ct = default)
    {
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