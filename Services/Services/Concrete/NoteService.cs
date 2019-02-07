using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Collections.Generic;

using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Domain.Concrete;
using VueServer.Models;
using VueServer.Services.Interface;
using VueServer.Models.Context;


namespace VueServer.Services.Concrete
{
    public class NoteService : INoteService
    {
        private readonly ILogger _logger;

        private readonly IUserService _user;

        private readonly WSContext _wsContext;
        
        public NoteService (ILoggerFactory logger,
            IUserService user, 
            WSContext wsContext)
        {
            _logger = logger?.CreateLogger<NoteService>() ?? throw new ArgumentNullException("Logger factory is null");
            _user = user ?? throw new ArgumentNullException("User service is null");
            _wsContext = wsContext ?? throw new ArgumentNullException("Webserver context is null");
        }

        public async Task<IResult<List<Notes>>> GetAll()
        {
            var notes = await _wsContext.Notes.ToListAsync();
            if (notes == null || notes.Count == 0) {
                _logger.LogInformation("[NoteService] GetAll: No notes found for all users");
                return new Result<List<Notes>>(null, Domain.Enums.StatusCode.NO_CONTENT);
            }

            return new Result<List<Notes>>(notes, Domain.Enums.StatusCode.OK);;
        }

        public async Task<IResult<List<Notes>>> Get()
        {
            var notes = await _wsContext.Notes.Where(a => a.UserId == _user.Name).ToListAsync();
            if (notes == null || notes.Count == 0) {
                _logger.LogInformation("[NoteService] Get: No notes found for current user");
                return new Result<List<Notes>>(new List<Notes>(), Domain.Enums.StatusCode.NO_CONTENT);
            }

            return new Result<List<Notes>>(notes, Domain.Enums.StatusCode.OK);;
        }

        public async Task<IResult<Notes>> Create(Notes note)
        {
            if (note == null)
            {
                _logger.LogWarning("[NoteService] Create: Note is null");
                return new Result<Notes>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var now = DateTimeOffset.UtcNow;
            note.UserId = _user.Name;
            note.Created = now;
            note.Updated = now;
            note.Id = 0;

            _wsContext.Notes.Add(note);

            try
            {
                await _wsContext.SaveChangesAsync();
            }
            catch(Exception)
            {
                _logger.LogError("[NoteService] Create: Error saving changes");
            }
            return new Result<Notes>(note, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Notes>> Update(Notes note)
        {
            if (note == null)
            {
                _logger.LogWarning("[NoteService] Update: Note is null");
                return new Result<Notes>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var updated = await _wsContext.Notes.Where(a => a.Id == note.Id && a.UserId == _user.Name).FirstOrDefaultAsync();
            if (updated == null) {
                _logger.LogWarning("[NoteService] Update: No note found to update");
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
            catch(Exception)
            {
                _logger.LogError("[NoteService] Update: Error saving changes");
            }

            return new Result<Notes>(updated, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult> Delete (int id)
        {
            var note = await _wsContext.Notes.Where(a => a.Id == id).FirstOrDefaultAsync();
            if (note == null)
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);

            try
            {
                _wsContext.Remove(note);
                await _wsContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("[NoteService] Delete: Error deleting note");
            }

            return new Result<IResult>(null, Domain.Enums.StatusCode.OK);
        }
    }
}
