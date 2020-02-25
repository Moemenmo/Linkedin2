﻿using Linkedin.DbContext;
using Linkedin.Models.Entites;
using LinkedIn.Core;
using LinkedIn.Core.Managers;
using LinkedIn.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LinkedIn.Web.Controllers
{
    public class NewsFeedController : Controller
    {


        public UnitOfWork UnitOfWork
        {
            get
            {
                return HttpContext.GetOwinContext().Get<UnitOfWork>();
            }
        }

        //public ApplicationUser loginuser
        //{
        //    get
        //    {
        //        return UnitOfWork.ApplicationUserManager.FindById(User.Identity.GetUserId());
        //    }
        //}

        // GET: NewsFeed
        [HttpGet]
        public ActionResult Index()
        {
            List<Post> darft = new List<Post>();
            var userManager = UnitOfWork.ApplicationUserManager;
            PostViewModel postVM = new PostViewModel();
            if (postVM.User.Posts!=null)
            {
                darft.AddRange(postVM.User.Posts);
            }

            foreach (var item in userManager.GetAllConnections(User.Identity.GetUserId().ToString()))
            {
                darft.AddRange(item.Posts);
            }
            //postVM.PagePosts.OrderBy(e => e.Date.Year)
            //    .ThenBy(e => e.Date.Month)
            //    .ThenBy(e => e.Date.Day)
            //    .ThenBy(e => e.Date.TimeOfDay.Hours)
            //    .ThenBy(e => e.Date.TimeOfDay.Minutes)
            //    .ThenBy(e => e.Date.TimeOfDay.Seconds);
            postVM.PagePosts=darft.OrderByDescending(d => Convert.ToDateTime(d.Date)).ToList();
            return View(postVM);
        }

        [HttpPost]
        public ActionResult AddPost(Post post, HttpPostedFileBase imgFile)
        {
            var userManager = UnitOfWork.ApplicationUserManager;
            var postManager = UnitOfWork.PostManager;

            if (ModelState.IsValid && (post.Status != null || imgFile != null))
            {
                if (imgFile != null)
                {
                    string extension = System.IO.Path.GetExtension(imgFile.FileName);
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(imgFile.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string path = "~/SavedImages/"+ fileName;
                    imgFile.SaveAs(System.IO.Path.Combine(Server.MapPath("~/SavedImages"), fileName));
                    post.ImageUrl = path;
                }
                post.AuthorId = User.Identity.GetUserId();
                post.Date = DateTime.Now;
                postManager.Add(post);
                post.Author = userManager.FindById(User.Identity.GetUserId());
                ModelState.Clear();
                return PartialView("_PostBody", post);
            }

            PostViewModel postVM = new PostViewModel();
            return View("Index", postVM);
        }
    }
}