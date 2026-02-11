using KobiMuhendislikTicket.Application.DTOs;

namespace KobiMuhendislikTicket.Application.Common
{
    /// <summary>
    /// Converts database entities to DTOs with proper timezone handling
    /// Ensures all DateTime fields are converted to Istanbul timezone (UTC+3)
    /// </summary>
    public static class DTOConverter
    {
        /// <summary>
        /// Converts a UTC DateTime to Istanbul timezone
        /// Used when DTOs might contain UTC times from old records
        /// </summary>
        public static DateTime EnsureLocalTime(DateTime dateTime)
        {
            return DateTimeHelper.ConvertToLocal(dateTime);
        }

        /// <summary>
        /// Converts a TicketCommentDto timestamps to local timezone
        /// </summary>
        public static TicketCommentDto ConvertCommentToLocal(TicketCommentDto comment)
        {
            if (comment == null) return null;

            return new TicketCommentDto
            {
                Id = comment.Id,
                Message = comment.Message,
                AuthorName = comment.AuthorName,
                IsAdminReply = comment.IsAdminReply,
                CreatedDate = EnsureLocalTime(comment.CreatedDate)
            };
        }

        /// <summary>
        /// Converts multiple comments to local timezone
        /// </summary>
        public static List<TicketCommentDto> ConvertCommentsToLocal(List<TicketCommentDto> comments)
        {
            if (comments == null) return null;
            return comments.Select(ConvertCommentToLocal).ToList();
        }

        /// <summary>
        /// Converts a TicketHistoryItemDto to local timezone
        /// </summary>
        public static TicketHistoryItemDto ConvertHistoryToLocal(TicketHistoryItemDto history)
        {
            if (history == null) return null;

            return new TicketHistoryItemDto
            {
                Description = history.Description,
                ActionBy = history.ActionBy,
                CreatedDate = EnsureLocalTime(history.CreatedDate)
            };
        }

        /// <summary>
        /// Converts multiple history items to local timezone
        /// </summary>
        public static List<TicketHistoryItemDto> ConvertHistoryToLocal(List<TicketHistoryItemDto> histories)
        {
            if (histories == null) return null;
            return histories.Select(ConvertHistoryToLocal).ToList();
        }
    }
}
