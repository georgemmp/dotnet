using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DatingApp.API.Models;
using System.Linq;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository repository;
        private readonly IMapper mapper;
        private readonly IOptions<CloudinarySettings> options;
        private Cloudinary cloudinary;

        public PhotosController(IDatingRepository repository, IMapper mapper,
                                IOptions<CloudinarySettings> options)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.options = options;

            Account acc = new Account (
                this.options.Value.CloudName,
                this.options.Value.ApiKey,
                this.options.Value.ApiSecret
            );

            this.cloudinary = new Cloudinary(acc);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, PhotoForCreationDto photo)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await this.repository.GetUser(userId);

            var file = photo.File;

            var uploadResult = new ImageUploadResult();

            if(file.Length > 0) 
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams() 
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500)
                            .Crop("fill").Gravity("face")
                    };

                    uploadResult = this.cloudinary.Upload(uploadParams);
                }
            }

            photo.Url = uploadResult.Uri.ToString();
            photo.PublicId = uploadResult.PublicId;

            var _photo = this.mapper.Map<Photo>(photo);

            if (!user.Photos.Any(u => u.IsMain))
            {
                _photo.IsMain = true;
            }

            user.Photos.Add(_photo);

            if (await this.repository.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Could not add the photo");
        }
    }
}