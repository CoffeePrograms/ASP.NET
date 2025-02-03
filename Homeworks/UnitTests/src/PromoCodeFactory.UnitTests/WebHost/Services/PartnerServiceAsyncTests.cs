using FluentAssertions;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.UnitTests.WebHost.Services.Builders;
using PromoCodeFactory.WebHost.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Services
{
    public class PartnerServiceAsyncTests
    {
        private readonly Mock<IRepository<Partner>> _mockRepository;
        private readonly PartnerService _partnerService;

        public PartnerServiceAsyncTests()
        {
            _mockRepository = new Mock<IRepository<Partner>>();
            _partnerService = new PartnerService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetPartnerOrThrowAsync_PartnerNotFound_ThrowsKeyNotFoundException()
        {
            var partnerId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.GetByIdAsync(partnerId)).ReturnsAsync((Partner)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _partnerService.GetPartnerOrThrowAsync(partnerId));
        }

        [Fact]
        public async Task GetPartnerOrThrowAsync_PartnerIsInactive_ThrowsInvalidOperationException()
        {
            var partner = new PartnerBuilder()
                .WithIsActive(false)
                .Build();
            _mockRepository.Setup(repo => repo.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _partnerService.GetPartnerOrThrowAsync(partner.Id));
        }

        [Fact]
        public async Task GetPartnerOrThrowAsync_PartnerIsActive_ReturnsPartner()
        {
            var partner = new PartnerBuilder()
                .WithIsActive(true)
                .Build();
            _mockRepository.Setup(repo => repo.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _partnerService.GetPartnerOrThrowAsync(partner.Id);

            result.Should().BeEquivalentTo(partner);
        }

        [Fact]
        public async void DeactivatePreviousLimit_ActiveLimitExists_DeactivatesLimitAndResetsPromoCodes()
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
        public async void DeactivatePreviousLimit_NoActiveLimit_DoesNothing()
        {
            var partner = new PartnerBuilder()
                .WithNumberIssuedPromoCodes(10)
                .Build();

            _partnerService.DeactivatePreviousLimit(partner);

            partner.PartnerLimits.Should().BeEmpty();
            partner.NumberIssuedPromoCodes.Should().Be(10);
        }

        [Fact]
        public async void CreateNewLimit_ValidRequest_ReturnsNewLimit()
        {
            var partner = new PartnerBuilder().Build();
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithLimit(100)
                .Build();

            var result = _partnerService.CreateNewLimit(partner, request).Result;

            result.Should().NotBeNull();
            result.Limit.Should().Be(request.Limit);
            result.PartnerId.Should().Be(partner.Id);
            result.CreateDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            result.EndDate.Should().Be(request.EndDate);
        }

        [Fact]
        public async Task CreateNewLimit_LimitIsZeroOrNegative_ThrowsArgumentException()
        {
            var partner = new PartnerBuilder().Build();
            var request = new SetPartnerPromoCodeLimitRequestBuilder()
                .WithLimit(0)
                .Build();

            await Assert.ThrowsAsync<ArgumentException>(() => _partnerService.CreateNewLimit(partner, request));
        }
    }
}
