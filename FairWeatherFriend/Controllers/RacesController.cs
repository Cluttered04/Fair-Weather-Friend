﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FairWeatherFriend.Data;
using FairWeatherFriend.Models;

namespace FairWeatherFriend.Controllers
{
    public class RacesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RacesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Races
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Race.Include(r => r.Track);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Races/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var race = await _context.Race
                .Include(r => r.Track)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (race == null)
            {
                return NotFound();
            }

            return View(race);
        }

        // GET: Races/Create
        public IActionResult Create()
        {
            ViewData["RaceTrackId"] = new SelectList(_context.RaceTrack, "Id", "Location");
            return View();
        }

        // POST: Races/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TimeOfDay,isCancelled,Prize,RaceTrackId")] Race race)
        {
            if (ModelState.IsValid)
            {
                _context.Add(race);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RaceTrackId"] = new SelectList(_context.RaceTrack, "Id", "Location", race.RaceTrackId);
            return View(race);
        }

        // GET: Races/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var race = await _context.Race.FindAsync(id);
            if (race == null)
            {
                return NotFound();
            }
            ViewData["RaceTrackId"] = new SelectList(_context.RaceTrack, "Id", "Location", race.RaceTrackId);
            return View(race);
        }

        // POST: Races/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TimeOfDay,isCancelled,Prize,RaceTrackId")] Race race)
        {
            if (id != race.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(race);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RaceExists(race.Id))
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
            ViewData["RaceTrackId"] = new SelectList(_context.RaceTrack, "Id", "Location", race.RaceTrackId);
            return View(race);
        }

        // GET: Races/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var race = await _context.Race
                .Include(r => r.Track)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (race == null)
            {
                return NotFound();
            }

            return View(race);
        }

        // POST: Races/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var race = await _context.Race.FindAsync(id);
            _context.Race.Remove(race);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RaceExists(int id)
        {
            return _context.Race.Any(e => e.Id == id);
        }
    }
}