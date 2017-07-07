using AutoMapper;
using CodeCamp.Api.Filters;
using CodeCamp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeCamp.Api.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
    public class SpeakersController : BaseController
    {
        private readonly ILogger<SpeakersController> _logger;
        private readonly IMapper _mapper;
        private readonly ICampRepository _repo;
        private readonly UserManager<CampUser> _userMgr;

        public SpeakersController(ICampRepository repo,
            ILogger<SpeakersController> logger,
            IMapper mapper,
            UserManager<CampUser> userMgr)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
            _userMgr = userMgr;
        }

        /// <summary>
        /// Example: http://localhost:8088/api/camps/ATL2016/speakers?includeTalks=true
        /// </summary>
        [HttpGet]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            IEnumerable<Speaker> speakers = includeTalks ? _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);
            return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
        }
        /// <summary>
        /// Example: http://localhost:8088/api/camps/ATL2016/speakers/1?includeTalks=true
        /// </summary>
        [HttpGet("{id}", Name = "SpeakerGet")]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            var speaker = includeTalks ? _repo.GetSpeakerWithTalks(id) : _repo.GetSpeaker(id);
            if (speaker == null) return NotFound($"Speaker with ID = '{id}' not found.");
            if (speaker.Camp.Moniker != moniker) return BadRequest($"Speaker not in Camp moniker = '{moniker}'.");

            return Ok(_mapper.Map<SpeakerModel>(speaker));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody] SpeakerModel model)
        {
            try
            {
                var camp = _repo.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest($"Couldn't find Camp with moniker: '{moniker}'.");

                Speaker speaker = _mapper.Map<Speaker>(model);
                speaker.Camp = camp;

                CampUser campUser = await _userMgr.FindByNameAsync(this.User.Identity.Name);
                if (campUser != null)
                {
                    speaker.User = campUser;

                    _repo.Add(speaker);

                    if (await _repo.SaveAllAsync())
                    {
                        var url = Url.Link("SpeakerGet", new { moniker = camp.Moniker, id = speaker.Id });
                        return Created(url, _mapper.Map<SpeakerModel>(speaker));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ex while adding speaker: {ex}");
            }

            return BadRequest("Couldn't add new speaker.");
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody]SpeakerModel model)
        {
            try
            {
                Speaker speaker = _repo.GetSpeaker(id);
                if (speaker == null) return NotFound($"Speaker with ID = '{id}' not found.");
                if (speaker.Camp.Moniker != moniker) return BadRequest($"Speaker not in Camp moniker = '{moniker}'.");

                if (speaker.User.UserName != this.User.Identity.Name) return Forbid();

                _mapper.Map(model, speaker);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<SpeakerModel>(speaker));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Ex in PUT speaker: {ex}");
            }

            return BadRequest("Failed to update speaker.");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                Speaker speaker = _repo.GetSpeaker(id);
                if (speaker == null) return NotFound($"Speaker with ID = '{id}' not found.");
                if (speaker.Camp.Moniker != moniker) return BadRequest($"Speaker not in Camp moniker = '{moniker}'.");

                if (speaker.User.UserName != this.User.Identity.Name) return Forbid();

                _repo.Delete(speaker);

                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ex in DELETE speaker: {ex}");
            }

            return BadRequest("Failed to delete speaker.");
        }
    }
}
