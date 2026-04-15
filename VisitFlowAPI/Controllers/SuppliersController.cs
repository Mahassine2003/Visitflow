using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.DTOs.Common;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly VisitFlowDbContext _db;

    public SuppliersController(VisitFlowDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<Supplier>>> GetAll([FromQuery] QueryParams query)
    {
        var q = _db.Suppliers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(x => x.CompanyName.Contains(query.Search) || x.Email.Contains(query.Search));

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return Ok(new PagedResult<Supplier> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Supplier>> GetById(int id)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<ActionResult<Supplier>> Create([FromBody] Supplier supplier)
    {
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<IActionResult> Update(int id, [FromBody] Supplier model)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
        if (supplier is null) return NotFound();
        supplier.CompanyName = model.CompanyName;
        supplier.Address = model.Address;
        supplier.Email = model.Email;
        supplier.Phone = model.Phone;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
        if (supplier is null) return NotFound();
        _db.Suppliers.Remove(supplier);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
