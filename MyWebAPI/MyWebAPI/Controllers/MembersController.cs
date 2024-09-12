using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebAPI.Models;

namespace MyWebAPI.Controllers
{
    [Route("apiMembers")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly SchoolProjectContext _context;

        public MembersController(SchoolProjectContext context)
        {
            _context = context;
        }

        // GET: api/Members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Members>>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Members>> GetMembers(string id)
        {
            var members = await _context.Members.FindAsync(id);

            if (members == null)
            {
                return NotFound();
            }

            return members;
        }

        // PUT: api/Members/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMembers(string id, Members members)
        {
            if (id != members.MemberID)
            {
                return BadRequest();
            }

            _context.Entry(members).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MembersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Members
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Members>> PostMembers(Members members)
        {
            _context.Members.Add(members);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MembersExists(members.MemberID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetMembers", new { id = members.MemberID }, members);
        }

        // DELETE: api/Members/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMembers(string id)
        {
            var members = await _context.Members.FindAsync(id);
            if (members == null)
            {
                return NotFound();
            }

            _context.Members.Remove(members);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Action 用來返回圖片
        [HttpGet("GetImage/{id}")]
        public IActionResult GetImage(string id)
        {
            // 從資料庫根據 MemberID 取得對應的圖片二進制資料
            var member = _context.Members.FirstOrDefault(m => m.MemberID == id);

            if (member == null || member.Photos == null || string.IsNullOrEmpty(member.ImageType))
            {
                return NotFound();
            }

            // 使用 FileContentResult 返回圖片的二進制數據和 MIME 類型
            return new FileContentResult(member.Photos, member.ImageType);
        }




        private bool MembersExists(string id)
        {
            return _context.Members.Any(e => e.MemberID == id);
        }
    }
}
