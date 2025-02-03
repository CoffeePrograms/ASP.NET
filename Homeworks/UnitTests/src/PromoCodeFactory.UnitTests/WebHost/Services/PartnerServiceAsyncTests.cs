using FluentAssertions;
using Moq;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.UnitTests.WebHost.Services.Builders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Services
{
    public partial class PartnerServiceTests
    {
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
    }
}
