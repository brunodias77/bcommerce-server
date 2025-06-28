using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")] // <-- MÁGICA ACONTECE AQUI!
public class DashboardController
{
    /// <summary>
    /// Retorna dados sumarizados para o painel administrativo.
    /// Este endpoint só pode ser acessado por usuários com a role 'Admin' no token.
    /// </summary>
    // [HttpGet]
    // public IActionResult GetDashboardSummary()
    // {
    //     // Em um caso real, você chamaria um UseCase para buscar os dados.
    //     var summary = new
    //     {
    //         TotalSales = 150320.75,
    //         NewOrders = 42,
    //         PendingReviews = 15,
    //         TopSellingProduct = "Tênis Esportivo Pro-Run"
    //     };
    //     
    //     return Ok(summary);
    // }
}