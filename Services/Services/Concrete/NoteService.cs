using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Collections.Generic;

using System.Threading.Tasks;
using VueServer.Common.Interface;
using VueServer.Common.Concrete;
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
            _logger = logger.CreateLogger<NoteService>();
            _user = user;
            _wsContext = wsContext;
        }

        public async Task<IResult<List<Notes>>> GetAll()
        {
            var notes = await _wsContext.Notes.ToListAsync();
            if (notes == null || notes.Count == 0) {
                _logger.LogInformation("[NoteService] GetAll: No notes found for all users");
                return new Result<List<Notes>>(null, Common.Enums.StatusCode.NO_CONTENT);
            }

            return new Result<List<Notes>>(notes, Common.Enums.StatusCode.OK);;
        }

        public async Task<IResult<List<Notes>>> Get()
        {
            //var username = _user.GetUsername();
            //if (string.IsNullOrWhiteSpace(username)) {
            //    _logger.LogWarning("Notes.Get: No username contained in the HttpContext");
            //    return BadRequest();
            //}

            var notes = await _wsContext.Notes.Where(a => a.User == _user.Name).ToListAsync();
            if (notes == null || notes.Count == 0) {
                _logger.LogInformation("[NoteService] Get: No notes found for current user");
                return new Result<List<Notes>>(new List<Notes>(), Common.Enums.StatusCode.NO_CONTENT);
            }

            return new Result<List<Notes>>(notes, Common.Enums.StatusCode.OK);;
        }

        public async Task<IResult<Notes>> Create(Notes note)
        {
            if (note == null)
            {
                _logger.LogWarning("[NoteService] Create: Note is null");
                return new Result<Notes>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            //var user = await _userContext.Users.Where(a => a.UserName == _user.Name).FirstOrDefaultAsync();
            //if (user == null) {
            //    _logger.LogWarning("Note.Create: No user found in the data store");
            //    return new Result<IResult>(null, Common.Enums.StatusCode.BAD_REQUEST);
            //}

            var now = DateTimeOffset.UtcNow;
            note.User = _user.Name;
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
            return new Result<Notes>(note, Common.Enums.StatusCode.OK);
        }

        public async Task<IResult<Notes>> Update(Notes note)
        {
            if (note == null)
            {
                _logger.LogWarning("[NoteService] Update: Note is null");
                return new Result<Notes>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            //var user = await _userContext.Users.Where(a => a.UserName == _user.Name).FirstOrDefaultAsync();
            //if (user == null) {
            //    _logger.LogWarning("Note.Update: No user found in the data store");
            //    return new Result<IResult>(null, Common.Enums.StatusCode.BAD_REQUEST);
            //}

            var updated = await _wsContext.Notes.Where(a => a.Id == note.Id && a.User == _user.Name).FirstOrDefaultAsync();
            if (updated == null) {
                _logger.LogWarning("[NoteService] Update: No note found to update");
                return new Result<Notes>(null, Common.Enums.StatusCode.BAD_REQUEST);
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

            return new Result<Notes>(updated, Common.Enums.StatusCode.OK);
        }

        public async Task<IResult> Delete (int id)
        {
            var note = await _wsContext.Notes.Where(a => a.Id == id).FirstOrDefaultAsync();
            if (note == null)
                return new Result<IResult>(null, Common.Enums.StatusCode.BAD_REQUEST);

            try
            {
                _wsContext.Remove(note);
                await _wsContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("[NoteService] Delete: Error deleting note");
            }

            return new Result<IResult>(null, Common.Enums.StatusCode.OK);
        }
    }
}
