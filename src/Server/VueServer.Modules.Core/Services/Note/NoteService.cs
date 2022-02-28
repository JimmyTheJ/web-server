using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Core.Objects;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Core.Services.User;

namespace VueServer.Modules.Core.Services.Note
{
    public class NoteService : INoteService
    {
        private readonly ILogger _logger;

        private readonly IUserService _user;

        private readonly IWSContext _wsContext;

        public NoteService(ILoggerFactory logger,
            IUserService user,
            IWSContext wsContext)
        {
            _logger = logger?.CreateLogger<NoteService>() ?? throw new ArgumentNullException("Logger factory is null");
            _user = user ?? throw new ArgumentNullException("User service is null");
            _wsContext = wsContext ?? throw new ArgumentNullException("Webserver context is null");
        }

        public async Task<IResult<List<Notes>>> GetAll()
        {
            var notes = await _wsContext.Notes.ToListAsync();
            if (notes == null || notes.Count == 0)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(GetAll)}: No notes found for all users");
                return new Result<List<Notes>>(null, Domain.Enums.StatusCode.NO_CONTENT);
            }

            return new Result<List<Notes>>(notes, Domain.Enums.StatusCode.OK); ;
        }

        public async Task<IResult<List<Notes>>> Get()
        {
            var notes = await _wsContext.Notes.Where(a => a.UserId == _user.Id).ToListAsync();
            if (notes == null || notes.Count == 0)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Get)}: No notes found for current user");
                return new Result<List<Notes>>(new List<Notes>(), Domain.Enums.StatusCode.NO_CONTENT);
            }

            return new Result<List<Notes>>(notes, Domain.Enums.StatusCode.OK); ;
        }

        public async Task<IResult<Notes>> Create(Notes note)
        {
            if (note == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Create)}: Note is null");
                return new Result<Notes>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var now = DateTimeOffset.UtcNow;
            note.UserId = _user.Id;
            note.Created = now;
            note.Updated = now;
            note.Id = 0;

            _wsContext.Notes.Add(note);

            try
            {
                await _wsContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(Create)}: Error saving changes");
            }
            return new Result<Notes>(note, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Notes>> Update(Notes note)
        {
            if (note == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Update)}: Note is null");
                return new Result<Notes>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var updated = await _wsContext.Notes.Where(a => a.Id == note.Id && a.UserId == _user.Id).FirstOrDefaultAsync();
            if (updated == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Update)}: No note found to update");
                return new Result<Notes>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            updated.Color = note.Color;
            updated.Text = note.Text;
            updated.Title = note.Title;
            updated.Type = note.Type;
            updated.Updated = DateTimeOffset.UtcNow;
            //updated.Updated = DateTime.UtcNow;

            try
            {
                await _wsContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(Update)}: Error saving changes");
            }

            return new Result<Notes>(updated, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult> Delete(int id)
        {
            var note = await _wsContext.Notes.Where(a => a.Id == id).FirstOrDefaultAsync();
            if (note == null)
            {
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            try
            {
                _wsContext.Remove(note);
                await _wsContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(Delete)}: Error deleting note");
            }

            return new Result<IResult>(null, Domain.Enums.StatusCode.OK);
        }
    }
}
