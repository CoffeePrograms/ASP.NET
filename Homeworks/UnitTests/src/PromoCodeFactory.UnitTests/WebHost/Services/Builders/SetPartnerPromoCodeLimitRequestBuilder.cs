using PromoCodeFactory.WebHost.Models;
using System;

namespace PromoCodeFactory.UnitTests.WebHost.Services.Builders
{
    public class SetPartnerPromoCodeLimitRequestBuilder
    {
        private readonly SetPartnerPromoCodeLimitRequest _request;

        public SetPartnerPromoCodeLimitRequestBuilder()
        {
            _request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(30)
            };
        }

        public SetPartnerPromoCodeLimitRequestBuilder WithLimit(int limit)
        {
            _request.Limit = limit;
            return this;
        }

        public SetPartnerPromoCodeLimitRequestBuilder WithEndDate(DateTime endDate)
        {
            _request.EndDate = endDate;
            return this;
        }

        public SetPartnerPromoCodeLimitRequest Build()
        {
            return _request;
        }
    }
}
