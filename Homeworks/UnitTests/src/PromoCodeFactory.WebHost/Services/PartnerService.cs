using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Services
{
    public class PartnerService
    {
        private readonly IRepository<Partner> _partnersRepository;

        public PartnerService(IRepository<Partner> partnersRepository)
        {
            _partnersRepository = partnersRepository;
        }

        public async Task<Partner> GetPartnerOrThrowAsync(Guid id)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);
            if (partner == null)
                throw new KeyNotFoundException("Партнер не найден");

            if (!partner.IsActive)
                throw new InvalidOperationException("Данный партнер не активен");

            return partner;
        }

        public void DeactivatePreviousLimit(Partner partner)
        {
            //Установка лимита партнеру
            var activeLimit = partner.PartnerLimits.FirstOrDefault(x => !x.CancelDate.HasValue);

            if (activeLimit != null)
            {
                //Если партнеру выставляется лимит, то мы 
                //должны обнулить количество промокодов, которые партнер выдал, если лимит закончился, 
                //то количество не обнуляется
                partner.NumberIssuedPromoCodes = 0;

                //При установке лимита нужно отключить предыдущий лимит
                activeLimit.CancelDate = DateTime.Now;
            }
        }

        public PartnerPromoCodeLimit CreateNewLimit(
            Partner partner, 
            SetPartnerPromoCodeLimitRequest request)
        {
            if (request.Limit <= 0)
                throw new ArgumentException("Лимит должен быть больше 0");

            return new PartnerPromoCodeLimit
            {
                Limit = request.Limit,
                Partner = partner,
                PartnerId = partner.Id,
                CreateDate = DateTime.Now,
                EndDate = request.EndDate
            };
        }
    }
}
