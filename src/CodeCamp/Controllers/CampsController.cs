using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System;
using System.Threading.Tasks;

namespace CodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : Controller
    {
        private readonly ILogger _logger;
        private readonly ICampRepository _repo;

        public CampsController(ICampRepository repo, ILogger<CampsController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = _repo.GetAllCamps();

            return Ok(camps);
        }

        [HttpGet("{id}", Name = "CampGet")]
        public IActionResult Get(int id, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;
                if (includeSpeakers)
                {
                    camp = _repo.GetCampWithSpeakers(id);
                }
                else
                {
                    camp = _repo.GetCamp(id);
                }

                if (camp == null)
                {
                    return NotFound($"Camp {id} was not found.");
                }

                return Ok(camp);
            }
            catch (Exception ex)
            {

            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Camp model)
        {
            try
            {
                _logger.LogInformation($"Creating na new Code Camp '{model.Name}'");
                _repo.Add(model);
                if (await _repo.SaveAllAsync())
                {
                    string newUri = Url.Link("CampGet", new { id = model.Id });
                    return Created(newUri, model);
                }
                else
                {
                    _logger.LogWarning("Could not save Camp to db");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception while saving Camp: {ex}");
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]Camp model)
        {
            try
            {
                Camp oldCamp = _repo.GetCamp(id);
                if (oldCamp == null)
                {
                    return NotFound($"Camp with id = {id} was not found.");
                }

                oldCamp.Name = model.Name ?? oldCamp.Name;
                oldCamp.Description = model.Description ?? oldCamp.Description;
                oldCamp.Location = model.Location ?? oldCamp.Location;
                oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
                oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

                if (await _repo.SaveAllAsync())
                {
                    return Ok(oldCamp);
                }
                else
                {
                    // not deleted
                    return BadRequest("Error while updating Camp");
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Camp oldCamp = _repo.GetCamp(id);
                if (oldCamp == null)
                {
                    return NotFound($"Couldn't find Camp with ID of '{id}'.");
                }

                _repo.Delete(oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
                else
                {
                    // not deleted
                }
            }
            catch (Exception)
            {

                throw;
            }

            return BadRequest("Couldn't delete Camp.");
        }
    }
}
