using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;

namespace PromoCodeFactory.UnitTests.WebHost.Services.Builders
{
    public class PartnerBuilder
    {
        private readonly Partner _partner;

        public PartnerBuilder()
        {
            _partner = new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Test Partner",
                IsActive = true,
                NumberIssuedPromoCodes = 0,
                PartnerLimits = new List<PartnerPromoCodeLimit>()
            };
        }

        public PartnerBuilder WithId(Guid id)
        {
            _partner.Id = id;
            return this;
        }

        public PartnerBuilder WithIsActive(bool isActive)
        {
            _partner.IsActive = isActive;
            return this;
        }

        public PartnerBuilder WithNumberIssuedPromoCodes(int numberIssuedPromoCodes)
        {
            _partner.NumberIssuedPromoCodes = numberIssuedPromoCodes;
            return this;
        }

        public PartnerBuilder WithPartnerLimits(List<PartnerPromoCodeLimit> limits)
        {
            _partner.PartnerLimits = limits;
            return this;
        }

        public Partner Build()
        {
            return _partner;
        }
    }
}
