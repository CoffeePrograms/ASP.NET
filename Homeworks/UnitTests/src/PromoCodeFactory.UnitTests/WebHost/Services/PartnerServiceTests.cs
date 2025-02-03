using FluentAssertions;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.UnitTests.WebHost.Services.Builders;
using PromoCodeFactory.WebHost.Services;
using System;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Services
{
    public partial class PartnerServiceTests
    {
        private readonly Mock<IRepository<Partner>> _mockRepository;
        private readonly PartnerService _partnerService;

        public PartnerServiceTests()
        {
            _mockRepository = new Mock<IRepository<Partner>>();
            _partnerService = new PartnerService(_mockRepository.Object);
        }

        [Fact]
        public void DeactivatePreviousLimit_ActiveLimitExists_DeactivatesLimitAndResetsPromoCodes()
        {
            var partner = new PartnerBuilder()
                .WithNumberIssuedPromoCodes(10)
                .Build();
            var activeLimit = new PartnerPromoCodeLimitBuilder()
                .WithCancelDate(null)
                .Build();
            partner.PartnerLimits.Add(activeLimit);

            _partnerService.DeactivatePreviousLimit(partner);

            activeLimit.CancelDate.Should().NotBeNull();
            partner.NumberIssuedPromoCodes.Should().Be(0);
        }

        [Fact]
        public void DeactivatePreviousLimit_NoActiveLimit_DoesNothing()
        {
            var partner = new PartnerBuilder()
                .WithNumberIssuedPromoCodes(10)
                .Build();

            _partnerService.DeactivatePreviousLimit(partner);

            partner.PartnerLimits.Should().BeEmpty();
            partner.NumberIssuedPromoCodes.Should().Be(10);
        }

        [Fact]
        public void CreateNewLimit_ValidRequest_ReturnsNewLimit()
        {
            var partner = new PartnerBuilder().Build();
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithLimit(100)
                .Build();

            var result = _partnerService.CreateNewLimit(partner, request);

            result.Should().NotBeNull();
            result.Limit.Should().Be(request.Limit);
            result.PartnerId.Should().Be(partner.Id);
            result.CreateDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            result.EndDate.Should().Be(request.EndDate);
        }

        [Fact]
        public void CreateNewLimit_LimitIsZeroOrNegative_ThrowsArgumentException()
        {
            var partner = new PartnerBuilder().Build();
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithLimit(0)
                .Build();

            Assert.Throws<ArgumentException>(() => _partnerService.CreateNewLimit(partner, request));
        }
    }
}
