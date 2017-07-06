using AutoMapper;
using CodeCamp.Api.Filters;
using CodeCamp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeCamp.Api.Controllers
{
    [Route("api/[controller]")]
    [ValidateModel]
    public class CampsController : BaseController
    {
        private readonly ILogger _logger;
        private readonly ICampRepository _repo;
        private readonly IMapper _mapper;

        public CampsController(ICampRepository repo,
            ILogger<CampsController> logger,
            IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = _repo.GetAllCamps();

            return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
        }

        [HttpGet("{moniker}", Name = "CampGet")]
        public IActionResult Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;
                if (includeSpeakers)
                {
                    camp = _repo.GetCampByMonikerWithSpeakers(moniker);
                }
                else
                {
                    camp = _repo.GetCampByMoniker(moniker);
                }

                if (camp == null)
                {
                    return NotFound($"Camp with moniker = '{moniker}' was not found.");
                }

                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch (Exception ex)
            {

            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CampModel model)
        {
            try
            {
                _logger.LogInformation($"Creating na new Code Camp '{model.Name}'");

                Camp camp = _mapper.Map<Camp>(model);

                _repo.Add(camp);
                if (await _repo.SaveAllAsync())
                {
                    string newUri = Url.Link("CampGet", new { moniker = camp.Moniker });
                    return Created(newUri, _mapper.Map<CampModel>(camp));
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

        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody]CampModel model)
        {
            try
            {
                Camp oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Camp with moniker = {moniker} was not found.");
                }

                _mapper.Map(model, oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampModel>(oldCamp));
                }
                else
                {
                    // not deleted
                    return BadRequest("Error while updating Camp");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ex updating Camp: {ex}");
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                Camp oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Couldn't find Camp with moniker of '{moniker}'.");
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
