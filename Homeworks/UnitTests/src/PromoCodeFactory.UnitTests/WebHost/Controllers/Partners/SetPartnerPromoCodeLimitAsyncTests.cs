using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models;
using PromoCodeFactory.WebHost.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IRepository<Partner>> _mockRepository;
        private readonly Mock<PartnerService> _mockService;
        private readonly PartnersController _controller;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            _fixture = new Fixture();
            // Игнорируем свойство Partner
            _fixture.Customize<PartnerPromoCodeLimit>(c => c.Without(x => x.Partner));
            // Игнорируем свойство PartnerLimits
            _fixture.Customize<Partner>(c => c.Without(x => x.PartnerLimits));
            _mockRepository = new();
            _mockService = new(_mockRepository.Object);
            _controller = new PartnersController(_mockRepository.Object, _mockService.Object);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerNotFound_ReturnsNotFound()
        {
            var partnerId = Guid.NewGuid();
            var request = _fixture.Create<SetPartnerPromoCodeLimitRequest>();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partnerId)).ReturnsAsync((Partner)null);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partnerId, request);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsInactive_ReturnsBadRequest()
        {
            var partner = _fixture.Build<Partner>()
                .With(p => p.IsActive, false)
                .Create();
            var request = _fixture.Create<SetPartnerPromoCodeLimitRequest>();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_LimitIsZeroOrNegative_ReturnsBadRequest()
        {
            var partner = _fixture.Build<Partner>()
                .With(p => p.IsActive, true)
                .Create();
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>()
                .With(r => r.Limit, 0)
                .Create();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ValidRequest_SetsNewLimitAndResetsIssuedPromoCodes()
        {
            var partner = _fixture.Build<Partner>()
                .With(p => p.IsActive, true)
                .With(p => p.NumberIssuedPromoCodes, 0)
                .Create();
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>()
                .With(r => r.Limit, 100)
                .Create();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            result.Should().BeOfType<CreatedAtActionResult>();
            partner.NumberIssuedPromoCodes.Should().Be(0);
            partner.PartnerLimits.Should().ContainSingle(limit => limit.Limit == request.Limit && limit.CancelDate == null);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ValidRequest_DeactivatesPreviousLimit()
        {
            var partner = _fixture.Build<Partner>()
                .With(p => p.IsActive, true)
                .Create();
            var previousLimit = _fixture.Build<PartnerPromoCodeLimit>()
                .With(l => l.CancelDate, (DateTime?)null)
                .Create();
            partner.PartnerLimits.Add(previousLimit);
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>()
                .With(r => r.Limit, 100)
                .Create();
            _mockRepository.Setup(rep => rep.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            result.Should().BeOfType<CreatedAtActionResult>();
            previousLimit.CancelDate.Should().NotBeNull();
        }
    }
}