using System;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Infrastructure;
using BackEnd.Repositories;
using ConferenceDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace BackEnd
{
    [Route("/api/[controller]")]
    [ApiController]
    public class AttendeesController : ControllerBase
    {
        private readonly IAttendeesRepository _attendeesRepository;
        private readonly ISessionsRepository _sessionsRepository;
        private readonly IDistributedCache _cache;

        private static readonly string _getAttendee = "GetAttendee";

        public AttendeesController(IAttendeesRepository attendeesRepository, ISessionsRepository sessionsRepository, IDistributedCache cache)
        {
            _attendeesRepository = attendeesRepository;
            _sessionsRepository = sessionsRepository;
            _cache = cache;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<AttendeeResponse>> Get(string username)
        {
            var attendee = await _attendeesRepository.GetByUsernameAsync(username);

            if (attendee == null)
            {
                return NotFound();
            }

            return attendee.MapAttendeeResponse();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AttendeeResponse>> Post(Attendee input)
        {
            // Check if the attendee already exists
            var existingAttendee = await _attendeesRepository.GetByUsernameAsync(input.UserName);

            if (existingAttendee != null)
            {
                return Conflict(input);
            }

            var attendee = await _attendeesRepository.AddAsync(new Data.Attendee
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                UserName = input.UserName,
                EmailAddress = input.EmailAddress
            });

            var result = attendee.MapAttendeeResponse();

            return CreatedAtAction(nameof(Get), new { username = result.UserName }, result);
        }

        [HttpPost("{username}/session/{sessionId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<AttendeeResponse>> AddSession(string username, int sessionId)
        {
            var attendee = await _attendeesRepository.GetByUsernameAsync(username);

            if (attendee == null)
            {
                return NotFound();
            }

            var session = await _sessionsRepository.GetByIdAsync(sessionId);

            if (session == null)
            {
                return BadRequest();
            }

            var newAttendee = await _attendeesRepository.AddSessionAsync(username, sessionId);

            return newAttendee.MapAttendeeResponse();
        }

        [HttpDelete("{username}/session/{sessionId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RemoveSession(string username, int sessionId)
        {
            var attendee = await _attendeesRepository.GetByUsernameAsync(username);

            if (attendee == null)
            {
                return NotFound();
            }

            var sessionAttendee = attendee.SessionAttendees.SingleOrDefault(sa => sa.SessionId == sessionId);

            if (sessionAttendee == null)
            {
                return BadRequest();
            }

            var success = attendee.SessionAttendees.Remove(sessionAttendee);

            await _attendeesRepository.UpdateAsync(attendee);

            return NoContent();
        }

        [HttpPost("{username}/image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<AttendeeResponse>> AddImage(string username, ImageRequest imageRequest)
        {
            var attendee = await _attendeesRepository.GetByUsernameAsync(username);

            if (attendee == null)
            {
                return NotFound();
            }

            attendee.AttendeeImages.Add(new Data.AttendeeImage
            {
                Attendee = attendee,
                Image = new Data.Image
                {
                    Name = imageRequest.Name,
                    Content = imageRequest.Content,
                    UploadDate = DateTimeOffset.Now,
                    ImageType = imageRequest.ImageType,
                }
            });

            await _attendeesRepository.UpdateAsync(attendee);

            var result = attendee.MapAttendeeResponse();

            return result;
        }

        private async Task CacheValue<T>(string key, T result)
        {
            var valueToCache = result.ToByteArray();
            var options = new DistributedCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromMinutes(1));
            await _cache.SetAsync(key, valueToCache, options);
        }
    }
}
