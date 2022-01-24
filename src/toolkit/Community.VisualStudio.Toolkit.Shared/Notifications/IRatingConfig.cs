using System.Threading.Tasks;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// An interface used to storing information related to <see cref="RatingPrompt"/>
    /// </summary>
    public interface IRatingConfig
    {
        /// <summary>
        /// The number of valid requests made to show the rating prompt.
        /// </summary>
        int RatingRequests { get; set; }

        /// <summary>
        /// A method to asynchronously persist the <see cref="RatingRequests"/>.
        /// </summary>
        /// <returns></returns>
        Task SaveAsync();
    }
}
