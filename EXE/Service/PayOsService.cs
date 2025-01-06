//using BusinessLogic.Models;
//using BusinessLogic.Services.External.Base;
//using Net.payOS;
//using Net.payOS.Types;
//using Options.Models;

//namespace BusinessLogic.Services.Externals.Implementation
//{
//    internal class PayOsService : IPayOsService
//    {
//        private readonly PayOsOptions _payOsOptions;

//        public PayOsService(PayOsOptions payOsOptions)
//        {
//            _payOsOptions = payOsOptions;
//        }

//        public async Task<bool> VerifyPaymentStatusByOrderCodeAsync(
//            long orderCode,
//            string paymentStatus
//        )
//        {
//            // Check if the input paymentStatus is null or not.
//            if (string.IsNullOrEmpty(paymentStatus))
//            {
//                return false;
//            }

//            var payOsConnection = CreatePayOsConnection();

//            try
//            {
//                // Verify if the paymentStatus that received from
//                // PayOS-API is similar to the input paymentStatus or not.
//                var paymentLinkInformation = await payOsConnection.getPaymentLinkInfomation(
//                    orderId: orderCode);

//                if (string.IsNullOrEmpty(paymentLinkInformation.status))
//                {
//                    return false;
//                }

//                return paymentStatus.Equals(paymentLinkInformation.status);
//            }
//            catch (Exception)
//            {
//                return false;
//            }
//        }

//        public async Task<AppResult<PaymentLinkInformation>> CancelPaymentLinkByOrderCodeAsync(
//            long transactionCode
//        )
//        {
//            var payOsConnection = CreatePayOsConnection();

//            try
//            {
//                var cancelResult = await payOsConnection.cancelPaymentLink(
//                    orderId: transactionCode,
//                    cancellationReason: "Product is out of stock");

//                return AppResult<PaymentLinkInformation>.Success(cancelResult);
//            }
//            catch (Exception)
//            {
//                return AppResult<PaymentLinkInformation>.Failed();
//            }
//        }

//        public async Task<AppResult<CreatePaymentResult>> CreatePaymentResultFromOrderAsync(
//            long orderCode,
//            int totalAmount
//        )
//        {
//            try
//            {
//                var payOsConnection = CreatePayOsConnection();

//                var paymentItems = new List<ItemData>
//                {
//                    new($"Đơn hàng [{orderCode}]", 1, totalAmount)
//                };

//                var paymentData = new PaymentData(
//                    orderCode: orderCode,
//                    amount: totalAmount,
//                    description: orderCode.ToString(),
//                    items: paymentItems,
//                    cancelUrl: _payOsOptions.CancelUrl,
//                    returnUrl: _payOsOptions.ReturnUrl
//                );

//                var result = await payOsConnection.createPaymentLink(paymentData: paymentData);

//                return AppResult<CreatePaymentResult>.Success(result);
//            }
//            catch (Exception ex)
//            {
//                return AppResult<CreatePaymentResult>.Failed(ex.Message);
//            }
//        }

//        #region Private Methods.
//        private PayOS CreatePayOsConnection()
//        {
//            return new PayOS(
//                clientId: _payOsOptions.ClientId,
//                apiKey: _payOsOptions.ApiKey,
//                checksumKey: _payOsOptions.ChecksumKey);
//        }
//        #endregion
//    }
//}
