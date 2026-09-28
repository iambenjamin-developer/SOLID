using Microsoft.AspNetCore.Mvc;

namespace SolidShop.Api.Features.Payments;

[ApiController]
[Route("api/payments")]
public class PaymentsController(IPaymentService payments) : ControllerBase
{
    [HttpGet("methods")]
    public ActionResult<IReadOnlyList<PaymentMethodResponse>> GetMethods() =>
        Ok(payments.GetAvailableMethods());
}
