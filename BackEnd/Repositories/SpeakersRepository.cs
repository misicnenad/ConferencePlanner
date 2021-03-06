﻿using BackEnd.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Repositories
{
    public class SpeakersRepository : ISpeakersRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SpeakersRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Speaker> AddAsync(Speaker speaker, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entityEntry = await _dbContext.Speakers.AddAsync(speaker, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return entityEntry.Entity;
        }

        public IQueryable<Speaker> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _dbContext.Speakers.AsNoTracking()
                                     .Include(s => s.SessionSpeakers)
                                        .ThenInclude(ss => ss.Session);
        }

        public async Task<Speaker> GetByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _dbContext.Speakers.AsNoTracking()
                                            .Include(s => s.SessionSpeakers)
                                                .ThenInclude(ss => ss.Session)
                                            .SingleOrDefaultAsync(s => s.ID == id, cancellationToken);
        }

        public async Task<Speaker> DeleteAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var speaker = await _dbContext.Speakers.Include(s => s.SessionSpeakers)
                                                        .ThenInclude(ss => ss.Session)
                                                    .SingleOrDefaultAsync(s => s.ID == id, cancellationToken);

            if (speaker != null)
            {
                _dbContext.Remove(speaker);

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return speaker;
        }

        public async Task<Speaker> UpdateAsync(Speaker updatedSpeaker, CancellationToken cancellationToken = default(CancellationToken))
        {
            var speaker = await _dbContext.FindAsync<Speaker>(new object[] { updatedSpeaker.ID }, cancellationToken);

            if (speaker != null)
            {
                speaker.Name = updatedSpeaker.Name;
                speaker.Bio = updatedSpeaker.Bio;
                speaker.WebSite = updatedSpeaker.WebSite;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return speaker;
        }
    }
}
