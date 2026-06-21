using System.ComponentModel.DataAnnotations;

namespace TmsApi
{
    public class PaymentOptions
    {
        [Required(ErrorMessage = "The GatewayUrl is strictly required for tuition processing.")]
        [Url(ErrorMessage = "The GatewayUrl must be a valid HTTP/HTTPS web address.")]
        public required string GatewayUrl { get; init; }

        [Range(100, 100000, ErrorMessage = "MaxDepositBirr must be between 100 and 100,000 Ethiopian Birr.")]
        public decimal MaxDepositBirr { get; init; }
    }
}