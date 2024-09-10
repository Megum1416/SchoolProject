using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolProject_DB.Models;

namespace SchoolProject_DB.Controllers
{
    public class RePostsController : Controller
    {
        private readonly SchoolProjectContext _context;

        public RePostsController(SchoolProjectContext context)
        {
            _context = context;
        }

        // GET: RePosts
        public async Task<IActionResult> Index()
        {
            var schoolProjectContext = _context.RePost.Include(r => r.Post);
            return View(await schoolProjectContext.ToListAsync());
        }

        // GET: RePosts/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rePost = await _context.RePost
                .Include(r => r.Post)
                .FirstOrDefaultAsync(m => m.RePostID == id);
            if (rePost == null)
            {
                return NotFound();
            }

            return View(rePost);
        }

        // GET: RePosts/Create
        public IActionResult Create()
        {
            ViewData["PostID"] = new SelectList(_context.Post, "PostID", "PostID");
            return View();
        }

        // POST: RePosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RePostID,PostID,Description,CreatedAt")] RePost rePost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rePost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostID"] = new SelectList(_context.Post, "PostID", "PostID", rePost.PostID);
            return View(rePost);
        }

        // GET: RePosts/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rePost = await _context.RePost.FindAsync(id);
            if (rePost == null)
            {
                return NotFound();
            }
            ViewData["PostID"] = new SelectList(_context.Post, "PostID", "PostID", rePost.PostID);
            return View(rePost);
        }

        // POST: RePosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("RePostID,PostID,Description,CreatedAt")] RePost rePost)
        {
            if (id != rePost.RePostID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rePost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RePostExists(rePost.RePostID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostID"] = new SelectList(_context.Post, "PostID", "PostID", rePost.PostID);
            return View(rePost);
        }

        // GET: RePosts/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rePost = await _context.RePost
                .Include(r => r.Post)
                .FirstOrDefaultAsync(m => m.RePostID == id);
            if (rePost == null)
            {
                return NotFound();
            }

            return View(rePost);
        }

        // POST: RePosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var rePost = await _context.RePost.FindAsync(id);
            if (rePost != null)
            {
                _context.RePost.Remove(rePost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RePostExists(string id)
        {
            return _context.RePost.Any(e => e.RePostID == id);
        }
    }
}
