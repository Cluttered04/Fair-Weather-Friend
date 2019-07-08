﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FairWeatherFriend.Data;
using FairWeatherFriend.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.AspNetCore.Identity;

namespace FairWeatherFriend.Controllers
{
    public class RacesController : Controller
    {
        private readonly ApplicationDbContext _context;


        private readonly UserManager<ApplicationUser> _userManager;
        public RacesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // GET: Races
        public async Task<IActionResult> Index(string searchQuery)
        {
            DateTime currentDate = DateTime.Now;
            var applicationDbContext = _context.Race.Include(r => r.Track).Where(r => DateTime.Compare(r.TimeOfDay, currentDate) > 0);

            if(searchQuery != null)
            {
                string normalizedSearchQuery = searchQuery.ToLower();
                applicationDbContext = applicationDbContext.Where(r => r.Track.Name.ToLower().Contains(normalizedSearchQuery));
                
            }

            applicationDbContext = applicationDbContext.OrderBy(r => r.TimeOfDay);
            return View(await applicationDbContext.ToListAsync());


        }

        public async Task<IActionResult> PastRaces(string searchQuery)
        {
            DateTime currentDate = DateTime.Now;
            var applicationDbContext = _context.Race.Include(r => r.Track).Where(r => DateTime.Compare(r.TimeOfDay, currentDate) < 0);
            if (searchQuery != null)
            {
                string normalizedSearchQuery = searchQuery.ToLower();
                applicationDbContext = applicationDbContext.Where(r => r.Track.Name.ToLower().Contains(normalizedSearchQuery));

            } 

            applicationDbContext = applicationDbContext.OrderByDescending(r => r.TimeOfDay);
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

            var raceWithOptInUsers = await _context.Race
        .Include(r => r.FavoriteRaces)
        .FirstOrDefaultAsync(m => m.Id == id);

            //var raceUsers = raceWithOptInUsers.FavoriteRaces;
            //var user = await GetCurrentUserAsync();
            //var breakpoint = raceUsers.Where(r => r.UserId == user.Id).Where(r => r.RaceId == race.Id);
            //var count = breakpoint.Count();

            return View(race);
        }

        // GET: Races/Create
        public IActionResult Create()
        {
            ViewData["RaceTrackId"] = new SelectList(_context.RaceTrack, "Id", "Name");
            return View();
        }

        // POST: Races/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TimeOfDay,isCancelled,Prize,RaceTrackId,Name,Laps")] Race race)
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,TimeOfDay,isCancelled,Prize,RaceTrackId,Name,Laps")] Race race)
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

                if(race.isCancelled == true)
                {
                    var raceWithOptInUsers = await _context.Race
                    .Include(r => r.FavoriteRaces)
                    .FirstOrDefaultAsync(m => m.Id == id);

                    var raceUsers = raceWithOptInUsers.FavoriteRaces;


                    var user = await GetCurrentUserAsync();

                    var breakpoint = raceUsers.Where(r => r.UserId == user.Id).Where(r => r.RaceId == race.Id);



                    SMSInformation twilio = new SMSInformation()
                    {
                        userPhone = user.PhoneNumber
                    };

                    string accountSid = twilio.sid;
                    string authToken = twilio.token;

                    TwilioClient.Init(accountSid, authToken);

                    var message = MessageResource.Create(
                        body: "Your race has been cancelled",
                        from: new Twilio.Types.PhoneNumber(twilio.phone),
                        to: new Twilio.Types.PhoneNumber(twilio.userPhone)
                    );
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

        public async Task<IActionResult> OptInNotification(int id)
        {
            var race = await _context.Race
                .Include(r => r.Track)
                .FirstOrDefaultAsync(m => m.Id == id);

            var user = await GetCurrentUserAsync();

            FavoriteRaces favoriteRace = new FavoriteRaces()
            {
                UserId = user.Id,
                RaceId = id
            };

            _context.Update(favoriteRace);
            await _context.SaveChangesAsync();

            return View(race);




            

        }

        private bool RaceExists(int id)
        {
            return _context.Race.Any(e => e.Id == id);
        }
    }
}
