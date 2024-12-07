using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC.Models;

// Pour le support du Blob pour les images
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using MVC.Data;
using MVC.Business;

namespace MVC.Controllers
{
    public class PostsController : Controller
    {
        private IRepository _repo;
        private BlobController _blobController;

        public PostsController(IRepository repo, BlobController blobController)
        {
            _repo = repo;
            _blobController = blobController;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {

            return View(await _repo.GetPostsIndex());
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Category,User,Created,FileToUpload")] PostForm postForm)
        {

            try
            {
                postForm.BlobImage = Guid.NewGuid();
                postForm.Url = await _blobController.PushImageToBlob(postForm.FileToUpload, (Guid)postForm.BlobImage);

                //retrait de l'erreur du au manque de l'imnage, celle-ci fut ajouter au model de base par notre CopyToAsync.
                ModelState.Remove("BlobImage");
                ModelState.Remove("Url");
            }
            catch (ExceptionFilesize)
            {
                // Fichier trop gros
                // ajout d'une erreur si le fichier est trop gros
                ModelState.AddModelError("FileToUpload", "Le fichier est trop gros.");
            }

            if (ModelState.IsValid)
            {
                await _repo.Add(postForm);

                return RedirectToAction(nameof(Index));
            }
            return View(postForm);
        }

        [HttpPost]
        // Function pour ajouter un like a un Post
        public async Task<ActionResult> Like(Guid id)
        {
            await _repo.IncrementPostLike(id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        // Fonction pour ajouter un dislike a un Post
        public async Task<ActionResult> Dislike(Guid id)
        {
            await _repo.IncrementPostDislike(id);

            return RedirectToAction("Index");
        }
    }
}
