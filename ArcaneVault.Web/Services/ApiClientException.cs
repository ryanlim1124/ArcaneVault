// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
namespace ArcaneVault.Web.Services;

/// <summary>
/// Thrown by <see cref="ApiClient"/> when a call to ArcaneVault.Api fails.
/// The message is always safe to display to the user - it is either the API's own
/// explanation ("A category with the code 'TCG' already exists.") or a friendly
/// fallback when the API cannot be reached at all.
/// </summary>
public class ApiClientException : Exception
{
    /// <summary>The HTTP status returned by the API, or null if the request never completed.</summary>
    public int? StatusCode { get; }

    public ApiClientException(string message, int? statusCode = null) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// True when the requested record does not exist. Used by the details/edit pages to
    /// redirect back to the list with an explanation, instead of showing an empty page.
    /// </summary>
    public bool IsNotFound => StatusCode == 404;
}
