using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;

namespace PromoCodeFactory.UnitTests.WebHost.Services.Builders
{
    public class PartnerPromoCodeLimitBuilder
    {
        private readonly PartnerPromoCodeLimit _limit;

        public PartnerPromoCodeLimitBuilder()
        {
            _limit = new PartnerPromoCodeLimit
            {
                Id = Guid.NewGuid(),
                PartnerId = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                CancelDate = null,
                Limit = 100
            };
        }

        public PartnerPromoCodeLimitBuilder WithCancelDate(DateTime? cancelDate)
        {
            _limit.CancelDate = cancelDate;
            return this;
        }

        public PartnerPromoCodeLimitBuilder WithLimit(int limit)
        {
            _limit.Limit = limit;
            return this;
        }

        public PartnerPromoCodeLimit Build()
        {
            return _limit;
        }
    }
}
