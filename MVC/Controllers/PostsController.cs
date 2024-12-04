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

namespace MVC.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Configuration pour recevoir les ApplicationConfiguration du AppConfig ...
        // Ici ce qui nous interesse c'est l'access au BlobConnectionString
        private ApplicationConfiguration _applicationConfiguration { get; }

        public PostsController(ApplicationDbContext context, IOptionsSnapshot<ApplicationConfiguration> options)
        {
            _context = context;
            _applicationConfiguration = options.Value;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            // Ajout d'un "order by", pour trier les resultats
            // Ajout d'un "take", pour prendre seulement une partie des entré, nous ferons une paginations plus tard.
            // Ajout d'un include pour ajouter a notre collection les commentaires lier a notre Post.
            return View(await _context.Posts.OrderByDescending(o => o.Created).Take(10).Include(i => i.Comments).ToListAsync());
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

            // Conversion du fichier recu en IFormFile a Byte[]. 
            // Ensuite le Byte[] sera envoyer au BlobStorage en utilisant un Guid comme identifiant.
            // Nous allons garder le Guid et créer un URL.
            using (MemoryStream ms = new MemoryStream())
            {
                if (ms.Length < 40971520)
                {
                    await postForm.FileToUpload.CopyToAsync(ms);

                    //Création du service connection au Blob
                    BlobServiceClient serviceClient = new BlobServiceClient(_applicationConfiguration.BlobConnectionString);

                    //Création du client pour le Blob
                    BlobContainerClient blobClient = serviceClient.GetBlobContainerClient(_applicationConfiguration.UnvalidatedBlob);

                    //Création d'un nom pour l'image
                    postForm.BlobImage = new Guid();

                    //Envoie de l'image sur le blob
                    await blobClient.UploadBlobAsync(postForm.Image.ToString(), ms);

                    //retrait de l'erreur du au manque de l'imnage, celle-ci fut ajouter au model de base par notre CopyToAsync.
                    ModelState.Remove("Image");
                }
                else
                {
                    //ajout d'une erreur si le fichier est trop gros
                    ModelState.AddModelError("FileToUpload", "Le fichier est trop gros.");
                }

            }

            if (ModelState.IsValid)
            {
                //le format de l'object envoyer ici va utiliser le polymorphise pour revenir a ça forme de base, ainsi il perdra le IFormFile.
                _context.Add(postForm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(postForm);
        }

        // Function pour ajouter un like a un Post
        public async Task<ActionResult> Like(int id)
        {
            // Utiliation du null-forgiving operator
            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-forgiving

            var post = await _context.Posts.FindAsync(id);
            post!.IncrementLike();
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Fonction pour ajouter un dislike a un Post
        public async Task<ActionResult> Dislike(int id)
        {
            // Utiliation du null-forgiving operator
            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-forgiving

            var post = await _context.Posts.FindAsync(id);

            post!.IncrementDislike();
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
