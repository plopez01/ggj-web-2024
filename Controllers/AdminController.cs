﻿using GGJWeb.Data;
using GGJWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using static GGJWeb.Controllers.PostController;

namespace GGJWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;

        public AdminController(ILogger<HomeController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "page")] int page)
        {
            var list = await _context.Posts!.OrderByDescending(p => p.PublishedOn).Skip(page * 5).Take(5).ToListAsync();
            HomeModel homeModel;

            try
            {
                homeModel = await _context.Home!.FirstAsync();
            } catch (InvalidOperationException) {
                homeModel = new HomeModel();
                DateTime now = DateTime.Now;

                // Remove milliseconds to prevent long datetime-local input
                homeModel.JamStart = now.Subtract(new TimeSpan(0, 0, 0, 0, now.Millisecond));
            }
            homeModel.posts = list;

            return View(homeModel);
        }


        // I left it off here, now I gota do the saving to db stuff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([Bind(nameof(NewHomeData.JamStart), nameof(NewHomeData.SignupLink))] NewHomeData data)
        {
            if (ModelState.IsValid)
            {
                // TODO AUTH
                /*if (info.Password != _config.GetValue<string>("Password"))
                {
                    return Unauthorized();
                }*/
                HomeModel homeModel;

                try
                {
                    // Modify if entry exists
                    homeModel = await _context.Home!.FirstAsync();
                    homeModel.JamStart = DateTime.SpecifyKind(data.JamStart, DateTimeKind.Utc);
                    homeModel.SignupLink = data.SignupLink;
                }
                catch (InvalidOperationException)
                {
                    // If not add new one
                    homeModel = new HomeModel();
                    homeModel.JamStart = DateTime.SpecifyKind(data.JamStart, DateTimeKind.Utc);
                    await _context.AddAsync(homeModel);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(HomeController.Index), "Admin");
            }
            return BadRequest();
        }

        public class NewHomeData
        {
            public string? SignupLink { get; set; }
            public DateTime JamStart { get; set; }
        }
    }
}
