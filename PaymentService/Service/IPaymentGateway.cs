using System;
using PaymentService.Dtos;

namespace PaymentService.Service;

public interface IPaymentGateway
{
    Task<GatewayPaymentResult> ChargeAsync(GatewayPaymentRequest request);
}

