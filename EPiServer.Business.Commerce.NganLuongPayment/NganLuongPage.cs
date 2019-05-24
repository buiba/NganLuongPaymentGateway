using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace EPiServer.Business.Commerce.NganLuongPayment
{
    [ContentType(GUID = "CA3AAF78-09AC-4FB7-A8A5-6BEDF978D06D",
            DisplayName = "Ngan Luong Payment Page",
            Description = "Ngan Luong Payment process page.",
            GroupName = "Payment",
            Order = 100)]    
    public class NganLuongPage : PageData
    {
    }
}
