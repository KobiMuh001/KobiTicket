using KobiMuhendislikTicket.Application.Common;

namespace KobiMuhendislikTicket.Domain.Common
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTimeHelper.GetLocalNow();
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; } = false; 
    }
}
