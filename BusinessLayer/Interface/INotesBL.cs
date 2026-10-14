using Models;
using RepositoryLayer.Entity;

namespace BusinessLayer.Interface;

public interface INotesBL
{
    Task<ResponseModel<NoteEntity>> AddNotes(
        NoteCreationModel model,
        int userId);

    Task<ResponseModel<List<NoteEntity>>> ViewNotes(int userId);

    Task<ResponseModel<NoteEntity>> ViewNotesById(
        int id, int userId);

    Task<ResponseModel<NoteEntity>> EditNote(
        int id, EditNoteModel model, int userId);

    Task<ResponseModel<bool>> DeleteNote(
        int id, int userId);

    Task<ResponseModel<bool>> ToggleArchive(
        int id, int userId);

    Task<ResponseModel<bool>> ToggleTrash(
        int id, int userId);
}