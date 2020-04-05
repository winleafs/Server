namespace Template.Models.Interfaces
{
    /// <summary>
    ///     Interface to define database classes.
    ///     Can be expanded with data needed for every database table.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        ///     The id of the entity gathered from the database.
        /// </summary>
        long Id { get; set; }
    }
}