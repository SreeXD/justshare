using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using JustShareBackend.Database;
using JustShareBackend.Models;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs.Models;

namespace JustShareBackend.Controllers
{
    [Route("[controller]")]
    public class PostsController : Controller
    {
        private JustShareDbContext justShareDbContext;
        private BlobServiceClient blobServiceClient;

        public PostsController(JustShareDbContext _justShareDbContext, BlobServiceClient _blobServiceClient)
        {
            justShareDbContext = _justShareDbContext;
            blobServiceClient = _blobServiceClient;
        }

        [HttpGet]
        public async Task<List<Post>> GetAllAsync()
        {
            return await justShareDbContext.Posts.ToListAsync();
        }

        [HttpPost("create")]
        public async Task<Post> CreateAsync()
        {
            HttpContext.Items.TryGetValue("userId", out var userId);

            var post = new Post
            {
                Description = Request.Form["description"],
                UserId = userId.ToString()
            };

            if (Request.Form.Files.Any())
            {
                var file = Request.Form.Files[0];

                if (file.ContentType.StartsWith("image/"))
                {
                    var extension = file.ContentType.Split("/")[1];

                    var fileStream = file.OpenReadStream();
                    var fileId = Guid.NewGuid().ToString() + "." + extension;

                    var containerClient = blobServiceClient.GetBlobContainerClient("files");
                    var blobClient = containerClient.GetBlobClient(fileId);

                    await blobClient.UploadAsync(fileStream, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = file.ContentType
                        }
                    });

                    post.ImageId = fileId;
                }
            }

            await justShareDbContext.Posts.AddAsync(post);
            await justShareDbContext.SaveChangesAsync();

            return post;
        }
    }
}
