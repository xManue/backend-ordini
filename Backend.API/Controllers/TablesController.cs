using Backend.Core.Models;
using Backend.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public TablesController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // ── GET all tables (admin) ──
        [HttpGet(Name = "GetAllTables")]
        public ActionResult<List<TableModel>> GetAll()
        {
            var tables = _appDbContext.Tables.OrderBy(t => t.Number).ToList();
            return Ok(tables);
        }

        // ── GET table info by token (public — used by client to validate their QR) ──
        [HttpGet("verify/{token}", Name = "VerifyTable")]
        public ActionResult VerifyTable(string token)
        {
            var table = _appDbContext.Tables.FirstOrDefault(t => t.Token == token && t.IsActive);
            if (table == null)
                return NotFound("Tavolo non trovato o disattivato.");

            return Ok(new { table.Number, table.Name });
        }

        // ── POST create table (admin) ──
        [HttpPost(Name = "CreateTable")]
        public ActionResult<TableModel> CreateTable([FromBody] TableDTO dto)
        {
            if (dto.Number <= 0)
                return BadRequest("Il numero tavolo deve essere maggiore di 0.");

            if (_appDbContext.Tables.Any(t => t.Number == dto.Number))
                return BadRequest($"Il tavolo {dto.Number} esiste già.");

            var table = new TableModel
            {
                Number = dto.Number,
                Name = dto.Name?.Trim() ?? $"Tavolo {dto.Number}",
                Token = Guid.NewGuid().ToString("N"),
                IsActive = true
            };

            _appDbContext.Tables.Add(table);
            _appDbContext.SaveChanges();

            return CreatedAtRoute("GetAllTables", null, table);
        }

        // ── PUT update table (admin) ──
        [HttpPut("{id}", Name = "UpdateTable")]
        public ActionResult UpdateTable(int id, [FromBody] TableDTO dto)
        {
            var table = _appDbContext.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
                return NotFound();

            if (dto.Number <= 0)
                return BadRequest("Il numero tavolo deve essere maggiore di 0.");

            if (_appDbContext.Tables.Any(t => t.Number == dto.Number && t.Id != id))
                return BadRequest($"Il tavolo {dto.Number} esiste già.");

            table.Number = dto.Number;
            table.Name = dto.Name?.Trim() ?? $"Tavolo {dto.Number}";
            _appDbContext.SaveChanges();

            return Ok(table);
        }

        // ── PATCH toggle active (admin) ──
        [HttpPatch("{id}/toggle", Name = "ToggleTable")]
        public ActionResult ToggleTable(int id)
        {
            var table = _appDbContext.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
                return NotFound();

            table.IsActive = !table.IsActive;
            _appDbContext.SaveChanges();

            return Ok(table);
        }

        // ── PATCH regenerate token (admin — invalidates old QR) ──
        [HttpPatch("{id}/regenerate", Name = "RegenerateToken")]
        public ActionResult RegenerateToken(int id)
        {
            var table = _appDbContext.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
                return NotFound();

            table.Token = Guid.NewGuid().ToString("N");
            _appDbContext.SaveChanges();

            return Ok(table);
        }

        // ── DELETE table (admin) ──
        [HttpDelete("{id}", Name = "DeleteTable")]
        public ActionResult DeleteTable(int id)
        {
            var table = _appDbContext.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
                return NotFound();

            _appDbContext.Tables.Remove(table);
            _appDbContext.SaveChanges();

            return NoContent();
        }
    }
}
